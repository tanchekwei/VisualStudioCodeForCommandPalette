// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Workspaces.Readers;

namespace WorkspaceLauncherForVSCode.Services
{
    public static class VisualStudioCodeWorkspaceProvider
    {
        public static async Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(VisualStudioCodeInstance instance, List<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken)
        {
            try
            {
#if DEBUG
                using var logger = new TimeLogger();
#endif
                if (cancellationToken.IsCancellationRequested || !File.Exists(instance.ExecutablePath))
                {
                    return new List<VisualStudioCodeWorkspace>();
                }

                var vscdbTask = VscdbWorkspaceReader.GetWorkspacesAsync(instance, cancellationToken);
                var storageJsonTask = StorageJsonWorkspaceReader.GetWorkspacesAsync(instance, cancellationToken);

                await Task.WhenAll(vscdbTask, storageJsonTask);

                var allWorkspaces = new List<VisualStudioCodeWorkspace>(vscdbTask.Result);
                allWorkspaces.AddRange(storageJsonTask.Result);

                var workspaceMap = new Dictionary<(string, WorkspaceType), VisualStudioCodeWorkspace>();
                foreach (var w in dbWorkspaces)
                {
                    if (w.Path != null)
                    {
                        workspaceMap[(w.Path, w.WorkspaceType)] = w;
                    }
                }

                foreach (var workspace in allWorkspaces)
                {
                    if (workspace.Path != null && workspaceMap.TryGetValue((workspace.Path, workspace.WorkspaceType), out var dbWorkspace))
                    {
                        workspace.Frequency = dbWorkspace.Frequency;
                        workspace.LastAccessed = dbWorkspace.LastAccessed;
                        workspace.PinDateTime = dbWorkspace.PinDateTime;
                    }
                }

                return allWorkspaces;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return new List<VisualStudioCodeWorkspace>();
            }
        }
    }
}
