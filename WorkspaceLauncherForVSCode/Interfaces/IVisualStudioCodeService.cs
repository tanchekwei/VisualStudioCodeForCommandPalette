// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode;

public interface IVisualStudioCodeService
{
    void LoadInstances(VisualStudioCodeEdition enabledEditions);

    List<VisualStudioCodeInstance> GetInstances();

    Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(IEnumerable<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken);

    Task<List<VisualStudioCodeWorkspace>> GetVisualStudioSolutions(IEnumerable<VisualStudioCodeWorkspace> dbWorkspaces, bool includeRegistry);
}