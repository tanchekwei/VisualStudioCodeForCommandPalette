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
        try
        {
            Icon = Classes.Icon.EraseTool;
            _workspaceStorage = workspaceStorage;
            _refreshWorkspacesCommand = refreshWorkspacesCommand;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    public override CommandResult Invoke()
    {
        try
        {
            _workspaceStorage.ResetAllFrequenciesAsync().GetAwaiter().GetResult();
            new ToastStatusMessage($"All item frequencies reset successfully").Show();
            return _refreshWorkspacesCommand.Invoke();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            new ToastStatusMessage($"Error resetting frequencies").Show();
            return CommandResult.KeepOpen();
        }
    }
}
