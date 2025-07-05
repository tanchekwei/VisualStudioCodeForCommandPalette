using System.Collections.Generic;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Services
{
    public class VisualStudioProviderWrapper : IVisualStudioProvider
    {
        public Task<List<VisualStudioCodeWorkspace>> GetSolutions(List<VisualStudioCodeWorkspace> dbWorkspaces, bool showPrerelease)
        {
            return VisualStudioProvider.GetSolutions(dbWorkspaces, showPrerelease);
        }
    }
}