// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
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
            try
            {
                _workspace = workspace;
                _pinService = pinService;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                throw;
            }
        }

        public override string Name => _workspace.PinDateTime.HasValue ? "Unpin from List" : "Pin to List";
        public override IconInfo Icon => _workspace.PinDateTime.HasValue ? Classes.Icon.Unpinned : Classes.Icon.Pinned;

        public override CommandResult Invoke()
        {
            try
            {
                if (_workspace.Path is null)
                {
                    return CommandResult.KeepOpen();
                }

                _ = _pinService.TogglePinStatusAsync(_workspace);

                return CommandResult.KeepOpen();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return CommandResult.KeepOpen();
            }
        }
    }
}