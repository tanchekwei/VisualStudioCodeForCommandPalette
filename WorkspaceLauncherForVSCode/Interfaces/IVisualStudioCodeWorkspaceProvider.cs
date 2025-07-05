using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Interfaces
{
    public interface IVisualStudioCodeWorkspaceProvider
    {
        Task<IEnumerable<VisualStudioCodeWorkspace>> GetWorkspacesAsync(VisualStudioCodeInstance instance, List<VisualStudioCodeWorkspace> dbWorkspaces, CancellationToken ct);
    }
}