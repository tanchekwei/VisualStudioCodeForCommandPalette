// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Commands
{
    internal sealed partial class PinWorkspaceCommand : InvokableCommand
    {
        private readonly VisualStudioCodeWorkspace _workspace;
        private readonly IPinService _pinService;

        public PinWorkspaceCommand(VisualStudioCodeWorkspace workspace, IPinService pinService)
        {
            _workspace = workspace;
            _pinService = pinService;
        }

        public override string Name => _workspace.PinDateTime.HasValue ? "Unpin from List" : "Pin to List";
        public override IconInfo Icon => _workspace.PinDateTime.HasValue ? Classes.Icon.Unpinned : Classes.Icon.Pinned;

        public override CommandResult Invoke()
        {
            if (_workspace.Path is null)
            {
                return CommandResult.KeepOpen();
            }

            _ = _pinService.TogglePinStatusAsync(_workspace.Path);

            return CommandResult.KeepOpen();
        }
    }
}