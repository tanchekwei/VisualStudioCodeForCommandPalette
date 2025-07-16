// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Services
{
    public class VisualStudioCodeService : IVisualStudioCodeService
    {
        public List<VisualStudioCodeInstance> Instances { get; private set; } = new List<VisualStudioCodeInstance>();

        public void LoadInstances(VisualStudioCodeEdition enabledEditions)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            Instances = VisualStudioCodeInstanceProvider.GetInstances(enabledEditions);
        }

        public List<VisualStudioCodeInstance> GetInstances()
        {
            return Instances;
        }

        public async Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(IEnumerable<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            if (Instances.Count == 1)
            {
                // Single instance: no need for concurrency or deduplication
                var instance = Instances[0];
                var workspaces = await VisualStudioCodeWorkspaceProvider.GetWorkspacesAsync(instance, dbWorkspaces.ToList(), cancellationToken);
                var unique = new Dictionary<string, VisualStudioCodeWorkspace>();
                foreach (var workspace in workspaces)
                {
                    if (workspace.Path == null) continue;
                    if (!unique.ContainsKey(workspace.Path))
                    {
                        unique[workspace.Path] = workspace;
                    }
                }
                return unique.Values.ToList();
            }
            else
            {
                // Multiple instances: use ConcurrentDictionary for thread-safe deduplication
                var workspaceMap = new ConcurrentDictionary<string, VisualStudioCodeWorkspace>();
                await Parallel.ForEachAsync(Instances, cancellationToken, async (instance, ct) =>
                {
                    var workspaces = await VisualStudioCodeWorkspaceProvider.GetWorkspacesAsync(instance, dbWorkspaces.ToList(), ct);
                    foreach (var workspace in workspaces)
                    {
                        if (workspace.Path == null) continue;
                        if (!workspaceMap.TryAdd(workspace.Path, workspace))
                        {
                            var existing = workspaceMap[workspace.Path];
                            if (existing.Source != workspace.Source)
                            {
                                existing.Source = VisualStudioCodeWorkspaceSource.StorageJsonVscdb;
                                if (workspace.SourcePath.Count > 0)
                                {
                                    existing.SourcePath.AddRange(workspace.SourcePath);
                                }
                            }
                        }
                    }
                });
                return workspaceMap.Values.ToList();
            }
        }

        public Task<List<VisualStudioCodeWorkspace>> GetVisualStudioSolutions(IEnumerable<VisualStudioCodeWorkspace> dbWorkspaces, bool includeRegistry)
        {
            return VisualStudioProvider.GetSolutions(dbWorkspaces.ToList(), includeRegistry);
        }
    }
}