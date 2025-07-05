// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Data.Sqlite;

namespace WorkspaceLauncherForVSCode.Classes
{
    public partial class WorkspaceStorage : IDisposable
    {
        private readonly SqliteConnection _connection;
        private SqliteCommand? _saveWorkspaceCommand;
        private const string DbName = "workspaces.db";

        private static class Queries
        {
            public const string Initialize = @"
                CREATE TABLE IF NOT EXISTS Workspaces (
                    Path TEXT PRIMARY KEY,
                    Name TEXT,
                    Type INTEGER,
                    Frequency INTEGER DEFAULT 0,
                    LastAccessed TEXT
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
FROM Workspaces w
LEFT JOIN PinnedWorkspaces p ON w.Path = p.Path;
";

            public const string SaveWorkspace = @"
                INSERT OR REPLACE INTO Workspaces (Path, Name, Type, Frequency, LastAccessed)
                VALUES (@Path, @Name, @Type, COALESCE((SELECT Frequency FROM Workspaces WHERE Path = @Path), 0), @LastAccessed)";

            public const string UpdateFrequency = "UPDATE Workspaces SET Frequency = Frequency + 1, LastAccessed = @LastAccessed WHERE Path = @path";

            public const string GetPinnedWorkspaces = "SELECT Path, PinDateTime FROM PinnedWorkspaces";
            public const string AddPinnedWorkspace = "INSERT OR REPLACE INTO PinnedWorkspaces (Path, PinDateTime) VALUES (@Path, @PinDateTime)";
            public const string RemovePinnedWorkspace = "DELETE FROM PinnedWorkspaces WHERE Path = @Path";
        }

        public WorkspaceStorage()
        {
            var dbPath = Path.Combine(Utilities.BaseSettingsPath(Constant.AppName), DbName);
            _connection = new SqliteConnection($"Data Source={dbPath}");
            _connection.Open();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = Queries.Initialize;
            command.ExecuteNonQuery();
        }

        public async Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync()
        {
            var workspaces = new List<VisualStudioCodeWorkspace>();
            using var command = _connection.CreateCommand();
            command.CommandText = Queries.GetWorkspaces;
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                workspaces.Add(new VisualStudioCodeWorkspace
                {
                    Path = reader.GetString(0),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    WorkspaceType = (Enums.WorkspaceType)reader.GetInt32(2),
                    Frequency = reader.GetInt32(3),
                    LastAccessed = reader.IsDBNull(4) ? DateTime.MinValue : DateTime.Parse(reader.GetString(4), CultureInfo.InvariantCulture),
                    PinDateTime = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5), CultureInfo.InvariantCulture),
                });
            }
            return workspaces;
        }

        public async Task SaveWorkspacesAsync(IEnumerable<VisualStudioCodeWorkspace> workspaces)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            using var transaction = _connection.BeginTransaction();

            if (_saveWorkspaceCommand == null)
            {
                _saveWorkspaceCommand = _connection.CreateCommand();
                _saveWorkspaceCommand.CommandText = Queries.SaveWorkspace;
                _saveWorkspaceCommand.Parameters.Add("@Path", SqliteType.Text);
                _saveWorkspaceCommand.Parameters.Add("@Name", SqliteType.Text);
                _saveWorkspaceCommand.Parameters.Add("@Type", SqliteType.Integer);
                _saveWorkspaceCommand.Parameters.Add("@LastAccessed", SqliteType.Text);
            }
            _saveWorkspaceCommand.Transaction = transaction;

            var pathParam = _saveWorkspaceCommand.Parameters["@Path"];
            var nameParam = _saveWorkspaceCommand.Parameters["@Name"];
            var typeParam = _saveWorkspaceCommand.Parameters["@Type"];
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
                lastAccessedParam.Value = workspace.LastAccessed.ToString("o");
                await _saveWorkspaceCommand.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }

        public async Task UpdateWorkspaceFrequencyAsync(string path)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = Queries.UpdateFrequency;
            command.Parameters.AddWithValue("@path", path);
            command.Parameters.AddWithValue("@LastAccessed", DateTime.Now.ToString("o"));
            await command.ExecuteNonQueryAsync();
        }

        public virtual async Task AddPinnedWorkspaceAsync(string path)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = Queries.AddPinnedWorkspace;
            command.Parameters.AddWithValue("@Path", path);
            command.Parameters.AddWithValue("@PinDateTime", DateTime.UtcNow.ToString("o"));
            await command.ExecuteNonQueryAsync();
        }

        public virtual async Task RemovePinnedWorkspaceAsync(string path)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = Queries.RemovePinnedWorkspace;
            command.Parameters.AddWithValue("@Path", path);
            await command.ExecuteNonQueryAsync();
        }

        public void Dispose()
        {
            _saveWorkspaceCommand?.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }
}