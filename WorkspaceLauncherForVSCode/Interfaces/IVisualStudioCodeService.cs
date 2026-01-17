// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models;

namespace WorkspaceLauncherForVSCode;

public interface IVisualStudioCodeService
{
    Task LoadInstancesAsync(VisualStudioCodeEdition enabledEditions, string? cursorPath = null, string? antigravityPath = null, string? windsurfPath = null);

    List<VisualStudioCodeInstance> GetInstances();

    List<VisualStudioInstance> GetVisualStudioInstances();

    Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync(IEnumerable<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken cancellationToken);

    Task<List<VisualStudioCodeWorkspace>> GetVisualStudioSolutions(IEnumerable<VisualStudioCodeWorkspace> dbWorkspaces, bool includeRegistry);
}