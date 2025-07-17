// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Workspaces.Models;

namespace WorkspaceLauncherForVSCode.Workspaces.Readers
{
    public static class StorageJsonWorkspaceReader
    {
        public static async Task<IEnumerable<VisualStudioCodeWorkspace>> GetWorkspacesAsync(VisualStudioCodeInstance instance, CancellationToken cancellationToken)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            var workspaces = new ConcurrentBag<VisualStudioCodeWorkspace>();
            var storageFilePath = Path.Combine(instance.StoragePath, "storage.json");

            if (!File.Exists(storageFilePath))
            {
                return workspaces;
            }

            try
            {
                await using var stream = File.OpenRead(storageFilePath);
                var root = await JsonSerializer.DeserializeAsync(stream, WorkspaceJsonContext.Default.StorageJsonRoot, cancellationToken);

                if (root?.BackupWorkspaces != null)
                {
                    if (root.BackupWorkspaces.Workspaces != null)
                    {
                        foreach (var workspace in root.BackupWorkspaces.Workspaces)
                        {
                            if (!string.IsNullOrEmpty(workspace.ConfigURIPath))
                            {
                                workspaces.Add(new VisualStudioCodeWorkspace(instance, workspace.ConfigURIPath, WorkspaceType.Workspace));
                            }
                        }
                    }

                    if (root.BackupWorkspaces.Folders != null)
                    {
                        foreach (var folder in root.BackupWorkspaces.Folders)
                        {
                            if (!string.IsNullOrEmpty(folder.FolderUri))
                            {
                                workspaces.Add(new VisualStudioCodeWorkspace(instance, folder.FolderUri, WorkspaceType.Folder));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing storage.json: {ex.Message}");
            }

            return workspaces;
        }

        public static async Task<int> RemoveWorkspaceAsync(VisualStudioCodeWorkspace workspace)
        {
            if (workspace.VSCodeInstance?.StoragePath is null)
            {
                return 0;
            }

            var storageFilePath = Path.Combine(workspace.VSCodeInstance.StoragePath, "storage.json");
            if (!File.Exists(storageFilePath)) return 0;

            var jsonString = await File.ReadAllTextAsync(storageFilePath);
            if (string.IsNullOrEmpty(jsonString)) return 0;

            var root = JsonSerializer.Deserialize(jsonString, WorkspaceJsonContext.Default.StorageJsonRoot);
            if (root?.BackupWorkspaces == null) return 0;

            int removedCount;
            if (workspace.WorkspaceType == WorkspaceType.Workspace)
            {
                removedCount = root.BackupWorkspaces.Workspaces?.RemoveAll(w => w.ConfigURIPath == workspace.Path) ?? 0;
            }
            else
            {
                removedCount = root.BackupWorkspaces.Folders?.RemoveAll(f => f.FolderUri == workspace.Path) ?? 0;
            }

            if (removedCount > 0)
            {
                var newJsonString = JsonSerializer.Serialize(root, WorkspaceJsonContext.Default.StorageJsonRoot);
                await File.WriteAllTextAsync(storageFilePath, newJsonString);
            }
            return removedCount;
        }
    }
}