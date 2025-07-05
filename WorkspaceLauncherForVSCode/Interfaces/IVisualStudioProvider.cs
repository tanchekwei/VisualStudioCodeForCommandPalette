using System.Collections.Generic;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Interfaces
{
    public interface IVisualStudioProvider
    {
        Task<List<VisualStudioCodeWorkspace>> GetSolutions(List<VisualStudioCodeWorkspace> dbWorkspaces, bool showPrerelease);
    }
}