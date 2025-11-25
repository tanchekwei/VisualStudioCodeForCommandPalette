// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Listeners;

namespace WorkspaceLauncherForVSCode;

public partial class WorkspaceLauncherForVSCodeCommandsProvider : CommandProvider
{
    private readonly SettingsManager _settingsManager;
    private readonly IVisualStudioCodeService _vscodeService;
    private readonly SettingsListener _settingsListener;
    private readonly VisualStudioCodePage _page;

    public WorkspaceLauncherForVSCodeCommandsProvider(SettingsManager settingsManager, IVisualStudioCodeService visualStudioCodeService, SettingsListener settingsListener, VisualStudioCodePage page)
    {
        try
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            _settingsManager = settingsManager;
            _vscodeService = visualStudioCodeService;
            DisplayName = "Visual Studio / Code";
#if DEBUG
            DisplayName += " (Dev)";
#endif
            Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
            Settings = _settingsManager.Settings;

            _settingsListener = settingsListener;
            _settingsListener.InstanceSettingsChanged += OnInstanceSettingsChanged;

            _page = page;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            _settingsManager = null!;
            _vscodeService = null!;
            _settingsListener = null!;
            _page = null!;
            throw;
        }
    }

    private void OnInstanceSettingsChanged(object? sender, EventArgs e)
    {
        try
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            _page.StartRefresh();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }

    public override ICommandItem[] TopLevelCommands()
    {
        try
        {
            return [
                new CommandItem(_page) {
                    Title = DisplayName,
                    MoreCommands = [
                        new CommandContextItem(Settings!.SettingsPage),
                    ],
                },
            ];
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return [];
        }
    }
}
