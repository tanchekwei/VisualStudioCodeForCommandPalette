// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Commands;

public sealed partial class RefreshWorkspacesCommand : InvokableCommand
{
    public override string Name => "Refresh";
    private readonly IVisualStudioCodeService _visualStudioCodeService;
    private readonly SettingsManager _settingsManager;
    public event EventHandler? TriggerRefresh;

    public RefreshWorkspacesCommand(IVisualStudioCodeService visualStudioCodeService, SettingsManager settingsManager)
    {
        try
        {
            Icon = new IconInfo("\xE72C"); // Refresh icon
            _visualStudioCodeService = visualStudioCodeService;
            _settingsManager = settingsManager;
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
            //_visualStudioCodePage.StartRefresh();
            TriggerRefresh?.Invoke(this, EventArgs.Empty);
            return CommandResult.KeepOpen();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return CommandResult.KeepOpen();
        }
    }
}
