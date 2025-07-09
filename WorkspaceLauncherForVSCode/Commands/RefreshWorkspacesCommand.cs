// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Commands;

public sealed partial class RefreshWorkspacesCommand : InvokableCommand
{
    public override string Name => "Refresh";
    private readonly IVisualStudioCodeService _visualStudioCodeService;
    private readonly SettingsManager _settingsManager;
    public event EventHandler? TriggerRefresh;

    public RefreshWorkspacesCommand(IVisualStudioCodeService visualStudioCodeService, SettingsManager settingsManager)
    {
        Icon = new IconInfo("\xE72C"); // Refresh icon
        _visualStudioCodeService = visualStudioCodeService;
        _settingsManager = settingsManager;
    }

    public override CommandResult Invoke()
    {
        //_visualStudioCodePage.StartRefresh();
        TriggerRefresh?.Invoke(this, EventArgs.Empty);
        return CommandResult.KeepOpen();
    }
}
