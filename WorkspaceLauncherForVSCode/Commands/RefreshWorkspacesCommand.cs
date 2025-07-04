// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Commands;

public sealed partial class RefreshWorkspacesCommand : InvokableCommand
{
    public override string Name => "Refresh";
    private readonly IVisualStudioCodeService _visualStudioCodeService;
    private readonly SettingsManager _settingsManager;
    private readonly VisualStudioCodePage _visualStudioCodePage;

    public RefreshWorkspacesCommand(IVisualStudioCodeService visualStudioCodeService, SettingsManager settingsManager, VisualStudioCodePage visualStudioCodePage)
    {
        Icon = Classes.Icon.Refresh;
        _visualStudioCodeService = visualStudioCodeService;
        _settingsManager = settingsManager;
        _visualStudioCodePage = visualStudioCodePage;
    }

    public override CommandResult Invoke()
    {
        _visualStudioCodePage.StartRefresh();
        return CommandResult.KeepOpen();
    }
}
