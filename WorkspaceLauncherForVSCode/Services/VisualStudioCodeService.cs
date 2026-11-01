// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Services.VisualStudio;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models;

namespace WorkspaceLauncherForVSCode.Services
{
    public class VisualStudioCodeService : IVisualStudioCodeService
    {
        private const int FirstInstanceIndex = 0;
        public List<VisualStudioCodeInstance> Instances { get; private set; } = new List<VisualStudioCodeInstance>();
        private readonly VisualStudioService _visualStudioService;

        public VisualStudioCodeService(VisualStudioService visualStudioService)
        {
            _visualStudioService = visualStudioService;
        }

        public async Task LoadInstancesAsync(VisualStudioCodeEdition enabledEditions, string? customPath = null, string? cursorPath = null, string? antigravityPath = null, string? windsurfPath = null, string? vscodiumPath = null)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            await _visualStudioService.InitInstancesAsync([]);
            Instances = await VisualStudioCodeInstanceProvider.GetInstancesAsync(enabledEditions, customPath, cursorPath, antigravityPath, windsurfPath, vscodiumPath);
        }

        public List<VisualStudioCodeInstance> GetInstances()
        {
            return Instances;
        }

        public List<VisualStudioInstance> GetVisualStudioInstances()
        {
            return _visualStudioService.Instances != null
                ? [.. _visualStudioService.Instances]
                : [];
        }

        public async Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(IEnumerable<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            var dbWorkspacesList = dbWorkspaces as List<VisualStudioCodeWorkspace> ?? new List<VisualStudioCodeWorkspace>(dbWorkspaces);

            if (Instances.Count == 1)
            {
                // Single instance: no need for concurrency or deduplication
                var firstInstance = Instances[FirstInstanceIndex];
                var workspaces = await VisualStudioCodeWorkspaceProvider.GetWorkspacesAsync(firstInstance, dbWorkspacesList, cancellationToken);
                var unique = new Dictionary<string, VisualStudioCodeWorkspace>(workspaces.Count, StringComparer.Ordinal);
                foreach (var workspace in workspaces)
                {
                    if (workspace.Path == null) continue;
                    unique.TryAdd(workspace.Path, workspace);
                }
                return new List<VisualStudioCodeWorkspace>(unique.Values);
            }
            else
            {
                // Multiple instances: use ConcurrentDictionary with value tuple key to avoid string allocations
                var workspaceMap = new ConcurrentDictionary<(string Path, WorkspaceType Type), VisualStudioCodeWorkspace>();
                await Parallel.ForEachAsync(Instances, cancellationToken, async (instance, ct) =>
                {
                    var workspaces = await VisualStudioCodeWorkspaceProvider.GetWorkspacesAsync(instance, dbWorkspacesList, ct);
                    foreach (var workspace in workspaces)
                    {
                        if (workspace.Path == null) continue;
                        workspaceMap.TryAdd((workspace.Path, workspace.WorkspaceType), workspace);
                    }
                });
                return new List<VisualStudioCodeWorkspace>(workspaceMap.Values);
            }
        }

        public Task<List<VisualStudioCodeWorkspace>> GetVisualStudioSolutions(IEnumerable<VisualStudioCodeWorkspace> dbWorkspaces, bool includeRegistry)
        {
            var dbWorkspacesList = dbWorkspaces as List<VisualStudioCodeWorkspace> ?? new List<VisualStudioCodeWorkspace>(dbWorkspaces);
            return VisualStudioProvider.GetSolutions(_visualStudioService, dbWorkspacesList, includeRegistry);
        }
    }
}