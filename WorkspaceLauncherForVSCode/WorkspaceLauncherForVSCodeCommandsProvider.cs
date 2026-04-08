// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Pages;

namespace WorkspaceLauncherForVSCode;

public partial class WorkspaceLauncherForVSCodeCommandsProvider : CommandProvider
{
    private readonly SettingsManager _settingsManager;
    private readonly IVisualStudioCodeService _vscodeService;
    private readonly SettingsListener _settingsListener;
    private readonly VisualStudioCodePage _page;
    private readonly IFallbackCommandItem[] _fallbacks;
    public WorkspaceLauncherForVSCodeCommandsProvider(
        SettingsManager settingsManager,
        IVisualStudioCodeService visualStudioCodeService,
        SettingsListener settingsListener,
        VisualStudioCodePage page,
        WorkspaceStorage workspaceStorage,
        RefreshWorkspacesCommand refreshWorkspacesCommand,
        IPinService pinService)
    {
        try
        {
            Id = Package.Current.Id.Name;
#if DEBUG
            using var logger = new TimeLogger();
#endif
            _settingsManager = settingsManager;
            _vscodeService = visualStudioCodeService;
            DisplayName = Constant.VisualStudioCodeDisplayName;
            Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
            Settings = _settingsManager.Settings;

            _settingsListener = settingsListener;
            _settingsListener.InstanceSettingsChanged += OnInstanceSettingsChanged;

            _page = page;

            var refreshCommandContextItem = new CommandContextItem(refreshWorkspacesCommand);
            _fallbacks = new IFallbackCommandItem[_settingsManager.FallbackCount + Constant.FallbackIndex.StartOfDynamicFallback];
            _fallbacks[Constant.FallbackIndex.OpenInVisualStudioCode] = new FallbackOpenRecentVisualStudioCodeItem(_page);
            for (int i = Constant.FallbackIndex.StartOfDynamicFallback, j = 0; i < _fallbacks.Length; i++, j++)
            {
                _fallbacks[i] = new FallbackWorkspaceItem(
                    _page,
                    _settingsManager,
                    workspaceStorage,
                    refreshCommandContextItem,
                    pinService,
                    j); // index start with 0
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            _settingsManager = null!;
            _vscodeService = null!;
            _settingsListener = null!;
            _page = null!;
            _fallbacks = null!;
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
    public override IFallbackCommandItem[] FallbackCommands() => _fallbacks;

    public override ICommandItem? GetCommandItem(string id)
    {
        return _page.GetCommandItem(id);
    }
}
