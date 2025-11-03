// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Services.VisualStudio;

namespace WorkspaceLauncherForVSCode.Services
{
    public static class VisualStudioProvider
    {
        public static Task<List<VisualStudioCodeWorkspace>> GetSolutions(
            VisualStudioService visualStudioService, List<VisualStudioCodeWorkspace> dbWorkspaces, bool showPrerelease)
        {
            try
            {
#if DEBUG
                using var logger = new TimeLogger();
#endif
                visualStudioService.InitInstances(Array.Empty<string>());
                var results = visualStudioService.GetResults(showPrerelease);

                var workspaceMap = new Dictionary<string, VisualStudioCodeWorkspace>();
                foreach (var w in dbWorkspaces)
                {
                    if (w.Path != null)
                    {
                        workspaceMap[w.Path] = w;
                    }
                }

                var list = new List<VisualStudioCodeWorkspace>();
                foreach (var r in results)
                {
                    var vs = new VisualStudioCodeWorkspace
                    {
                        Name = r.Name,
                        Path = r.FullPath,
                        WindowsPath = r.FullPath,
                        WorkspaceType = WorkspaceType.Solution,
                        VSInstance = r.Instance,
                        VSLastAccessed = r.LastAccessed,
                    };
                    vs.SetWorkspaceType();
                    if (r.FullPath != null && workspaceMap.TryGetValue(r.FullPath, out var workspace))
                    {
                        vs.Frequency = workspace.Frequency;
                        vs.LastAccessed = workspace.LastAccessed;
                        vs.PinDateTime = workspace.PinDateTime;
                    }
                    list.Add(vs);
                }

                return Task.FromResult(list);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return Task.FromResult(new List<VisualStudioCodeWorkspace>());
            }
        }
    }
}
