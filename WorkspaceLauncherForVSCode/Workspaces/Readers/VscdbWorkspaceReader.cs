// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Workspaces.Models;

namespace WorkspaceLauncherForVSCode.Workspaces.Readers
{
    public static class VscdbWorkspaceReader
    {
        public static async Task<IEnumerable<VisualStudioCodeWorkspace>> GetWorkspacesAsync(VisualStudioCodeInstance instance, CancellationToken cancellationToken)
        {
            try
            {
#if DEBUG
                using var logger = new TimeLogger();
#endif
                var workspaces = new List<VisualStudioCodeWorkspace>();
                var dbPath = Path.Combine(instance.StoragePath, "state.vscdb");

                if (!File.Exists(dbPath))
                {
                    return workspaces;
                }

                try
                {
                    using (var db = new VscdbDatabase(dbPath))
                    {
                        await db.OpenAsync(cancellationToken);
                        var jsonString = await db.ReadWorkspacesJsonAsync(cancellationToken);
                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                            var root = await JsonSerializer.DeserializeAsync(stream, WorkspaceJsonContext.Default.VscdbRoot, cancellationToken);

                            if (root?.Entries != null)
                            {
                                foreach (var entry in root.Entries)
                                {
                                    VisualStudioCodeWorkspace? workspace = null;
                                    var folderType = instance.VisualStudioCodeType == VisualStudioCodeType.Insider ? WorkspaceType.FolderInsider :
                                                     instance.VisualStudioCodeType == VisualStudioCodeType.Cursor ? WorkspaceType.Cursor :
                                                     instance.VisualStudioCodeType == VisualStudioCodeType.Antigravity ? WorkspaceType.Antigravity :
                                                     WorkspaceType.Folder;

                                    var workspaceType = instance.VisualStudioCodeType == VisualStudioCodeType.Insider ? WorkspaceType.WorkspaceInsider :
                                                        instance.VisualStudioCodeType == VisualStudioCodeType.Cursor ? WorkspaceType.Cursor :
                                                        instance.VisualStudioCodeType == VisualStudioCodeType.Antigravity ? WorkspaceType.Antigravity :
                                                        WorkspaceType.Workspace;

                                    if (!string.IsNullOrEmpty(entry.FolderUri))
                                    {
                                        workspace = new VisualStudioCodeWorkspace(instance, entry.FolderUri, folderType);
                                    }
                                    else if (entry.Workspace != null && !string.IsNullOrEmpty(entry.Workspace.ConfigPath))
                                    {
                                        workspace = new VisualStudioCodeWorkspace(instance, entry.Workspace.ConfigPath, workspaceType);
                                    }

                                    if (workspace != null)
                                    {
                                        workspaces.Add(workspace);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex);
                }

                return workspaces;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return new List<VisualStudioCodeWorkspace>();
            }
        }

        public static async Task<int> RemoveWorkspaceAsync(VisualStudioCodeWorkspace workspace)
        {
            try
            {
                if (workspace.VSCodeInstance?.StoragePath is null)
                {
                    return 0;
                }
                var dbPath = Path.Combine(workspace.VSCodeInstance.StoragePath, "state.vscdb");
                if (!File.Exists(dbPath)) return 0;

                await using var connection = new SqliteConnection($"Data Source={dbPath};");
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT value FROM ItemTable WHERE key LIKE 'history.recentlyOpenedPathsList'";
                var jsonString = (string?)await command.ExecuteScalarAsync();

                if (string.IsNullOrEmpty(jsonString)) return 0;

                var root = JsonSerializer.Deserialize(jsonString, WorkspaceJsonContext.Default.VscdbRoot);
                if (root?.Entries == null) return 0;

                var removedCount = root.Entries.RemoveAll(entry =>
                    ((workspace.WorkspaceType == WorkspaceType.Folder ||
                      workspace.WorkspaceType == WorkspaceType.FolderInsider ||
                      workspace.WorkspaceType == WorkspaceType.Cursor ||
                      workspace.WorkspaceType == WorkspaceType.Antigravity) && entry.FolderUri == workspace.Path) ||
                    ((workspace.WorkspaceType == WorkspaceType.Workspace ||
                      workspace.WorkspaceType == WorkspaceType.WorkspaceInsider ||
                      workspace.WorkspaceType == WorkspaceType.Cursor ||
                      workspace.WorkspaceType == WorkspaceType.Antigravity) && entry.Workspace?.ConfigPath == workspace.Path));

                if (removedCount > 0)
                {
                    var newJsonString = JsonSerializer.Serialize(root, WorkspaceJsonContext.Default.VscdbRoot);
                    command.CommandText = "UPDATE ItemTable SET value = @value WHERE key LIKE 'history.recentlyOpenedPathsList'";
                    command.Parameters.AddWithValue("@value", newJsonString);

                    await command.ExecuteNonQueryAsync();
                }
                return removedCount;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return 0;
            }
        }
    }
}