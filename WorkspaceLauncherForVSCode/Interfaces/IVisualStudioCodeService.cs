// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode
{
    public interface IVisualStudioCodeService
    {
        List<VisualStudioCodeInstance> Instances { get; }
        void LoadInstances(VisualStudioCodeEdition enabledEditions);
        Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(List<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken);
        Task<List<VisualStudioCodeWorkspace>> GetVisualStudioSolutions(List<VisualStudioCodeWorkspace> dbWorkspaces, bool showPrerelease);
    }
}