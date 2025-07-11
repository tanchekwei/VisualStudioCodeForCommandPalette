// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Commands;

public sealed partial class ResetFrequencyCommand : InvokableCommand
{
    public override string Name => "Reset Frequency";
    private readonly WorkspaceStorage _workspaceStorage;
    private readonly RefreshWorkspacesCommand _refreshWorkspacesCommand;
    public ResetFrequencyCommand(WorkspaceStorage workspaceStorage, RefreshWorkspacesCommand refreshWorkspacesCommand)
    {
        Icon = Classes.Icon.EraseTool;
        _workspaceStorage = workspaceStorage;
        _refreshWorkspacesCommand = refreshWorkspacesCommand;
    }

    public override CommandResult Invoke()
    {
        _workspaceStorage.ResetAllFrequenciesAsync().GetAwaiter().GetResult();
        new ToastStatusMessage($"All item frequencies reset successfully").Show();
        return _refreshWorkspacesCommand.Invoke();
    }
}
