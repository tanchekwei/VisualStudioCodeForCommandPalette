// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Commands;

public sealed partial class ResetDatabaseCommand : InvokableCommand
{
    public override string Name => "Reset Extension Database";
    private readonly WorkspaceStorage _workspaceStorage;
    private readonly Action _refreshAction;

    public ResetDatabaseCommand(WorkspaceStorage workspaceStorage, Action refreshAction)
    {
        try
        {
            Icon = Classes.Icon.EraseTool;
            _workspaceStorage = workspaceStorage;
            _refreshAction = refreshAction;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    public override CommandResult Invoke()
    {
        var confirmArgs = new ConfirmationArgs()
        {
            Title = "Reset Extension Database",
            Description = "Are you sure you want to reset the extension database?\n\nThis only resets the extension's cached records, including pinned items and usage counters (used for record ranking), and refreshes the workspace list (workspaces.db).\n\nYour actual Visual Studio and Visual Studio Code files and configurations are untouched and safe.",
            PrimaryCommand = new ResetDatabaseActionCommand(_workspaceStorage, _refreshAction),
            IsPrimaryCommandCritical = true,
        };

        return CommandResult.Confirm(confirmArgs);
    }

    private sealed partial class ResetDatabaseActionCommand : InvokableCommand
    {
        public override string Name => "Reset Extension Database";
        private readonly WorkspaceStorage _workspaceStorage;
        private readonly Action _refreshAction;

        public ResetDatabaseActionCommand(WorkspaceStorage workspaceStorage, Action refreshAction)
        {
            _workspaceStorage = workspaceStorage;
            _refreshAction = refreshAction;
        }

        public override CommandResult Invoke()
        {
            try
            {
                _workspaceStorage.ResetDatabaseAsync().GetAwaiter().GetResult();
                new ToastStatusMessage("Extension database reset successfully").Show();
                _refreshAction();
                return CommandResult.GoBack();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                new ToastStatusMessage("Error resetting database").Show();
                return CommandResult.KeepOpen();
            }
        }
    }
}