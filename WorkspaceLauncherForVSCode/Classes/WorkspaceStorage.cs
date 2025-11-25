// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Data.Sqlite;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Classes
{
    public sealed partial class WorkspaceStorage : IDisposable
    {
        private readonly SqliteConnection _connection;
        private SqliteCommand? _saveWorkspaceCommand;
        private const string DbName = "workspaces.db";

        private static class Queries
        {
            public const string Initialize = @"
                CREATE TABLE IF NOT EXISTS WorkspacesV2 (
                    Path TEXT,
                    Name TEXT,
                    Type INTEGER,
                    Frequency INTEGER DEFAULT 0,
                    LastAccessed TEXT,
                    PRIMARY KEY (Path, Type)
                );
                CREATE TABLE IF NOT EXISTS PinnedWorkspaces (
                    Path TEXT PRIMARY KEY,
                    PinDateTime TEXT NOT NULL
                );";

            public const string GetWorkspaces = @"SELECT 
    w.Path, 
    w.Name, 
    w.Type, 
    w.Frequency, 
    w.LastAccessed, 
    p.PinDateTime
FROM WorkspacesV2 w
LEFT JOIN PinnedWorkspaces p ON w.Path = p.Path;
";

            public const string SaveWorkspace = @"
                INSERT OR REPLACE INTO WorkspacesV2 (Path, Name, Type, Frequency, LastAccessed)
                VALUES (
                    @Path,
                    @Name,
                    @Type,
                    COALESCE((SELECT Frequency FROM WorkspacesV2 WHERE Path = @Path AND Type = @Type), @Frequency),
                    COALESCE((SELECT LastAccessed FROM WorkspacesV2 WHERE Path = @Path AND Type = @Type), @LastAccessed)
                );
                ";

            public const string UpdateFrequency = "UPDATE WorkspacesV2 SET Frequency = Frequency + 1, LastAccessed = @LastAccessed WHERE Path = @path AND Type = @Type";

            public const string GetPinnedWorkspaces = "SELECT Path, PinDateTime FROM PinnedWorkspaces";
            public const string AddPinnedWorkspace = "INSERT OR REPLACE INTO PinnedWorkspaces (Path, PinDateTime) VALUES (@Path, @PinDateTime)";
            public const string RemovePinnedWorkspace = "DELETE FROM PinnedWorkspaces WHERE Path = @Path";

            public const string ResetAllFrequencies = "UPDATE WorkspacesV2 SET Frequency = 0";
        }

        public WorkspaceStorage()
        {
            try
            {
                var dbPath = Path.Combine(Utilities.BaseSettingsPath(Constant.AppName), DbName);
                _connection = new SqliteConnection($"Data Source={dbPath}");
                _connection.Open();
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                throw;
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                using var transaction = _connection.BeginTransaction();

                // 1. Initialize V2 Table and PinnedWorkspaces
                var initCmd = _connection.CreateCommand();
                initCmd.Transaction = transaction;
                initCmd.CommandText = Queries.Initialize;
                initCmd.ExecuteNonQuery();

                // 2. Check if old 'Workspaces' table exists (indicating legacy schema or previous version)
                var checkCmd = _connection.CreateCommand();
                checkCmd.Transaction = transaction;
                checkCmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Workspaces'";
                var count = checkCmd.ExecuteScalar() as long? ?? 0L;
                var oldTableExists = count > 0;

                if (oldTableExists)
                {
                    // 3. Migrate data from 'Workspaces' to 'WorkspacesV2'
                    var migrateCmd = _connection.CreateCommand();
                    migrateCmd.Transaction = transaction;
                    migrateCmd.CommandText = @"
                        INSERT OR IGNORE INTO WorkspacesV2 (Path, Name, Type, Frequency, LastAccessed)
                        SELECT Path, Name, Type, Frequency, LastAccessed FROM Workspaces;
                        DROP TABLE Workspaces;
                    ";
                    migrateCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        public async Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync()
        {
            var workspaces = new List<VisualStudioCodeWorkspace>();
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = Queries.GetWorkspaces;
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    workspaces.Add(new VisualStudioCodeWorkspace
                    {
                        Path = reader.GetString(0),
                        Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                        WorkspaceType = (WorkspaceType)reader.GetInt32(2),
                        Frequency = reader.GetInt32(3),
                        LastAccessed = reader.IsDBNull(4) ? DateTime.MinValue : DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture),
                        PinDateTime = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture),
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
            return workspaces;
        }

        public async Task SaveWorkspacesAsync(IEnumerable<VisualStudioCodeWorkspace> workspaces)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            try
            {
                using var transaction = _connection.BeginTransaction();

                if (_saveWorkspaceCommand == null)
                {
                    _saveWorkspaceCommand = _connection.CreateCommand();
                    _saveWorkspaceCommand.CommandText = Queries.SaveWorkspace;
                    _saveWorkspaceCommand.Parameters.Add("@Path", SqliteType.Text);
                    _saveWorkspaceCommand.Parameters.Add("@Name", SqliteType.Text);
                    _saveWorkspaceCommand.Parameters.Add("@Type", SqliteType.Integer);
                    _saveWorkspaceCommand.Parameters.Add("@Frequency", SqliteType.Integer);
                    _saveWorkspaceCommand.Parameters.Add("@LastAccessed", SqliteType.Text);
                }
                _saveWorkspaceCommand.Transaction = transaction;

                var pathParam = _saveWorkspaceCommand.Parameters["@Path"];
                var nameParam = _saveWorkspaceCommand.Parameters["@Name"];
                var typeParam = _saveWorkspaceCommand.Parameters["@Type"];
                var frequencyParam = _saveWorkspaceCommand.Parameters["@Frequency"];
                var lastAccessedParam = _saveWorkspaceCommand.Parameters["@LastAccessed"];

                foreach (var workspace in workspaces)
                {
                    if (string.IsNullOrEmpty(workspace.Path))
                    {
                        continue;
                    }

                    pathParam.Value = workspace.Path;
                    nameParam.Value = workspace.Name ?? (object)DBNull.Value;
                    typeParam.Value = (int)workspace.WorkspaceType;
                    frequencyParam.Value = 0;
                    lastAccessedParam.Value = workspace.LastAccessed.ToString("o");
                    await _saveWorkspaceCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        public async Task UpdateWorkspaceFrequencyAsync(string path, WorkspaceType type)
        {
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = Queries.UpdateFrequency;
                command.Parameters.AddWithValue("@path", path);
                command.Parameters.AddWithValue("@Type", (int)type);
                command.Parameters.AddWithValue("@LastAccessed", DateTime.Now.ToString("o"));
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        public async Task ResetAllFrequenciesAsync()
        {
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = Queries.ResetAllFrequencies;
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        public async Task AddPinnedWorkspaceAsync(string path)
        {
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = Queries.AddPinnedWorkspace;
                command.Parameters.AddWithValue("@Path", path);
                command.Parameters.AddWithValue("@PinDateTime", DateTime.UtcNow.ToString("o"));
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        public async Task RemovePinnedWorkspaceAsync(string path)
        {
            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = Queries.RemovePinnedWorkspace;
                command.Parameters.AddWithValue("@Path", path);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        public void Dispose()
        {
            _saveWorkspaceCommand?.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }
}