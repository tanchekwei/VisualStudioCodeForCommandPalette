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
            try
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
                        var folderType = instance.VisualStudioCodeType == VisualStudioCodeType.Insider ? WorkspaceType.FolderInsider :
                                         instance.VisualStudioCodeType == VisualStudioCodeType.Cursor ? WorkspaceType.CursorFolder :
                                         instance.VisualStudioCodeType == VisualStudioCodeType.Antigravity ? WorkspaceType.AntigravityFolder :
                                         instance.VisualStudioCodeType == VisualStudioCodeType.Windsurf ? WorkspaceType.WindsurfFolder :
                                         WorkspaceType.Folder;

                        var workspaceType = instance.VisualStudioCodeType == VisualStudioCodeType.Insider ? WorkspaceType.WorkspaceInsider :
                                            instance.VisualStudioCodeType == VisualStudioCodeType.Cursor ? WorkspaceType.CursorWorkspace :
                                            instance.VisualStudioCodeType == VisualStudioCodeType.Antigravity ? WorkspaceType.AntigravityWorkspace :
                                            instance.VisualStudioCodeType == VisualStudioCodeType.Windsurf ? WorkspaceType.WindsurfWorkspace :
                                            WorkspaceType.Workspace;

                        if (root.BackupWorkspaces.Workspaces != null)
                        {
                            foreach (var workspace in root.BackupWorkspaces.Workspaces)
                            {
                                if (!string.IsNullOrEmpty(workspace.ConfigURIPath))
                                {
                                    workspaces.Add(new VisualStudioCodeWorkspace(instance, workspace.ConfigURIPath, workspaceType));
                                }
                            }
                        }

                        if (root.BackupWorkspaces.Folders != null)
                        {
                            foreach (var folder in root.BackupWorkspaces.Folders)
                            {
                                if (!string.IsNullOrEmpty(folder.FolderUri))
                                {
                                    workspaces.Add(new VisualStudioCodeWorkspace(instance, folder.FolderUri, folderType));
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
                return new ConcurrentBag<VisualStudioCodeWorkspace>();
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

                var storageFilePath = Path.Combine(workspace.VSCodeInstance.StoragePath, "storage.json");
                if (!File.Exists(storageFilePath)) return 0;

                var jsonString = await File.ReadAllTextAsync(storageFilePath);
                if (string.IsNullOrEmpty(jsonString)) return 0;

                var root = JsonSerializer.Deserialize(jsonString, WorkspaceJsonContext.Default.StorageJsonRoot);
                if (root?.BackupWorkspaces == null) return 0;

                int removedCount;
                if (workspace.WorkspaceType == WorkspaceType.Workspace ||
                    workspace.WorkspaceType == WorkspaceType.WorkspaceInsider ||
                    workspace.WorkspaceType == WorkspaceType.CursorWorkspace ||
                    workspace.WorkspaceType == WorkspaceType.AntigravityWorkspace ||
                    workspace.WorkspaceType == WorkspaceType.WindsurfWorkspace)
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
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return 0;
            }
        }
    }
}