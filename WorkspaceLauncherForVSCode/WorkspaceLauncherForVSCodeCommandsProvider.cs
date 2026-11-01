// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Pages;

namespace WorkspaceLauncherForVSCode;

public partial class WorkspaceLauncherForVSCodeCommandsProvider : CommandProvider
{
    private readonly SettingsManager _settingsManager;
    private readonly SettingsListener _settingsListener;
    private readonly VisualStudioCodePage _page;
    private readonly IFallbackCommandItem[] _fallbacks;
    private readonly ICommandItem[] _topLevelCommands;

    public WorkspaceLauncherForVSCodeCommandsProvider(
        SettingsManager settingsManager,
        SettingsListener settingsListener,
        VisualStudioCodePage page)
    {
        try
        {
            Id = Package.Current.Id.Name;
#if DEBUG
            using var logger = new TimeLogger();
#endif
            _settingsManager = settingsManager;
            DisplayName = Constant.VisualStudioCodeDisplayName;
            Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
            Settings = _settingsManager.Settings;

            _settingsListener = settingsListener;
            _settingsListener.InstanceSettingsChanged += OnInstanceSettingsChanged;

            _page = page;

            _topLevelCommands = [
                new CommandItem(_page) {
                    MoreCommands = [
                        new CommandContextItem(Settings!.SettingsPage),
                    ],
                },
            ];

            _fallbacks = new IFallbackCommandItem[_settingsManager.FallbackCount + Constant.FallbackIndex.StartOfDynamicFallback];
            _fallbacks[Constant.FallbackIndex.OpenInVisualStudioCode] = new FallbackOpenRecentVisualStudioCodeItem(_page);
            for (int i = Constant.FallbackIndex.StartOfDynamicFallback, fallbackResultIndex = 0; i < _fallbacks.Length; i++, fallbackResultIndex++)
            {
                _fallbacks[i] = new FallbackWorkspaceItem(
                    _page,
                    fallbackResultIndex); // index start with 0
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            _settingsManager = null!;
            _settingsListener = null!;
            _page = null!;
            _fallbacks = null!;
            _topLevelCommands = null!;
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

    public override ICommandItem[] TopLevelCommands() => _topLevelCommands;

    public override IFallbackCommandItem[] FallbackCommands() => _fallbacks;

    public override ICommandItem? GetCommandItem(string id)
    {
        return _page.GetCommandItem(id);
    }
}