// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;

namespace WorkspaceLauncherForVSCode.Classes
{
    public static class VscdbRecentListChangeTrackerInitializer
    {
        private static readonly HashSet<string> _initializedDbs = new HashSet<string>();
        private static readonly object _lock = new object();

        public static void Initialize(string vscdbPath)
        {
            lock (_lock)
            {
                if (_initializedDbs.Contains(vscdbPath))
                {
                    return;
                }

                using (var connection = new SqliteConnection($"Data Source={vscdbPath}"))
                {
                    connection.Open();

                    // Create the tracker table if it doesn't exist
                    var createTableCommand = connection.CreateCommand();
                    createTableCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS CmdPalVisualStudioCodeHistoryRecentlyOpenedPathsListTracker (
                            version INTEGER NOT NULL
                        );
                    ";
                    createTableCommand.ExecuteNonQuery();

                    // Insert a default value if the table is empty
                    var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = @"
                        INSERT INTO CmdPalVisualStudioCodeHistoryRecentlyOpenedPathsListTracker (version)
                        SELECT 1
                        WHERE NOT EXISTS (
                            SELECT 1 FROM CmdPalVisualStudioCodeHistoryRecentlyOpenedPathsListTracker
                        );
                    ";
                    insertCommand.ExecuteNonQuery();

                    // Create the trigger to update the tracker table
                    var triggerCommand = connection.CreateCommand();
                    triggerCommand.CommandText = @"
                        CREATE TRIGGER IF NOT EXISTS CmdPalVisualStudioCodeHistoryRecentlyOpenedPathsListTrigger
                        AFTER INSERT ON ItemTable
                        WHEN NEW.key = 'history.recentlyOpenedPathsList'
                        BEGIN
                            UPDATE CmdPalVisualStudioCodeHistoryRecentlyOpenedPathsListTracker SET version = version + 1;
                        END;
                    ";
                    triggerCommand.ExecuteNonQuery();
                }

                _initializedDbs.Add(vscdbPath);
            }
        }
    }
}