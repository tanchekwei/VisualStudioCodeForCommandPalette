using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Interfaces
{
    public interface IWorkspaceStorage : IDisposable
    {
        Task<List<VisualStudioCodeWorkspace>> GetWorkspacesAsync();
        Task SaveWorkspacesAsync(IEnumerable<VisualStudioCodeWorkspace> workspaces);
        Task UpdateWorkspaceFrequencyAsync(string path);
        Task AddPinnedWorkspaceAsync(string path);
        Task RemovePinnedWorkspaceAsync(string path);
    }
}