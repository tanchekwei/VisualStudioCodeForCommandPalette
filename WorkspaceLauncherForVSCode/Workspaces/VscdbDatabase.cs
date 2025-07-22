// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Workspaces.Readers
{
    public partial class VscdbDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public VscdbDatabase(string dbPath)
        {
            try
            {
                _connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly;");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                _connection = null!;
            }
        }

        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _connection.OpenAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        public async Task<string> ReadWorkspacesJsonAsync(CancellationToken cancellationToken)
        {
            try
            {
                var command = _connection.CreateCommand();
                command.CommandText = "SELECT value FROM ItemTable WHERE key LIKE 'history.recentlyOpenedPathsList'";
                var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return reader.GetString(0);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return string.Empty;
            }
        }

        public void Dispose()
        {
            try
            {
                _connection.Dispose();
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }
    }
}