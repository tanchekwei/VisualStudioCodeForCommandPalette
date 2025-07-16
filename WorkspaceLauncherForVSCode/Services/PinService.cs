// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Services
{
    public class PinService : IPinService
    {
        private readonly WorkspaceStorage _workspaceStorage;
        public event EventHandler? RefreshList;

        public PinService(WorkspaceStorage workspaceStorage)
        {
            _workspaceStorage = workspaceStorage;
        }

        public async Task TogglePinStatusAsync(VisualStudioCodeWorkspace workspace)
        {
            if (workspace?.Path is null)
            {
                return;
            }

            if (workspace.PinDateTime.HasValue)
            {
                await _workspaceStorage.RemovePinnedWorkspaceAsync(workspace.Path);
                workspace.PinDateTime = null;
                new ToastStatusMessage($"Unpinned \"{workspace.Name}\"").Show();
            }
            else
            {
                await _workspaceStorage.AddPinnedWorkspaceAsync(workspace.Path);
                workspace.PinDateTime = DateTime.UtcNow;
                new ToastStatusMessage($"Pinned \"{workspace.Name}\"").Show();
            }

            RefreshList?.Invoke(this, EventArgs.Empty);
        }
    }
}