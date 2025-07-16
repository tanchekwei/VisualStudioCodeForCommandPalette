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
        private readonly VisualStudioCodePage _page;
        private readonly WorkspaceStorage _workspaceStorage;

        public PinService(VisualStudioCodePage page, WorkspaceStorage workspaceStorage)
        {
            _page = page;
            _workspaceStorage = workspaceStorage;
        }

        public async Task TogglePinStatusAsync(string path)
        {
            var workspace = _page.AllWorkspaces.FirstOrDefault(w => w.Path == path);
            if (workspace == null)
            {
                return;
            }

            if (workspace.PinDateTime.HasValue)
            {
                await _workspaceStorage.RemovePinnedWorkspaceAsync(path);
                workspace.PinDateTime = null;
                new ToastStatusMessage($"Unpinned \"{workspace.Name}\"").Show();
            }
            else
            {
                await _workspaceStorage.AddPinnedWorkspaceAsync(path);
                workspace.PinDateTime = DateTime.UtcNow;
                new ToastStatusMessage($"Pinned \"{workspace.Name}\"").Show();
            }

            _page.RefreshList();
        }
    }
}