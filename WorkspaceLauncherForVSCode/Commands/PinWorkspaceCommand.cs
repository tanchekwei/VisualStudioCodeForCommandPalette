// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Commands
{
    public sealed partial class PinWorkspaceCommand : InvokableCommand
    {
        private readonly VisualStudioCodeWorkspace _workspace;
        private readonly VisualStudioCodePage _page;
        private readonly IWorkspaceStorage _workspaceStorage;

        public PinWorkspaceCommand(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page, IWorkspaceStorage workspaceStorage)
        {
            _workspace = workspace;
            _page = page;
            _workspaceStorage = workspaceStorage;
        }

        public override string Name => _workspace.PinDateTime.HasValue ? "Unpin from List" : "Pin to List";
        public override IconInfo Icon => _workspace.PinDateTime.HasValue ? Classes.Icon.Unpinned : Classes.Icon.Pinned;

        public override CommandResult Invoke()
        {
            if (_workspace.Path is null)
            {
                return CommandResult.KeepOpen();
            }

            _ = Task.Run(async () =>
            {
                if (_workspace.PinDateTime.HasValue)
                {
                    await _workspaceStorage.RemovePinnedWorkspaceAsync(_workspace.Path);
                }
                else
                {
                    await _workspaceStorage.AddPinnedWorkspaceAsync(_workspace.Path);
                }

                await _page.TogglePinStatus(_workspace.Path);

                var statusMessage = _workspace.PinDateTime.HasValue ? $"Pinned \"{_workspace.Name}\"" : $"Unpinned \"{_workspace.Name}\"";
                new ToastStatusMessage(statusMessage).Show();
            });

            return CommandResult.KeepOpen();
        }
    }
}