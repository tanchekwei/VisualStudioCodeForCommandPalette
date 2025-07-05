using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Services
{
    public class VisualStudioCodeWorkspaceProviderWrapper : IVisualStudioCodeWorkspaceProvider
    {
        public Task<IEnumerable<VisualStudioCodeWorkspace>> GetWorkspacesAsync(VisualStudioCodeInstance instance, List<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken ct)
        {
            return VisualStudioCodeWorkspaceProvider.GetWorkspacesAsync(instance, dbWorkspaces, ct);
        }
    }
}