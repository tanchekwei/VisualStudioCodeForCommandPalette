// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
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
#if DEBUG
            using var logger = new TimeLogger();
#endif
            visualStudioService.InitInstances(Array.Empty<string>());
            var results = visualStudioService.GetResults(showPrerelease);
            var workspaceMap = dbWorkspaces.Where(w => w.Path != null).ToDictionary(w => w.Path!, w => w);

            var list = results.Select(r =>
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
                return vs;
            }).ToList();
            return Task.FromResult(list);
        }
    }
}