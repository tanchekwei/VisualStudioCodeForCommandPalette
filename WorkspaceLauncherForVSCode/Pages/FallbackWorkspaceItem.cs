// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models;
using WorkspaceLauncherForVSCode.Workspaces;

namespace WorkspaceLauncherForVSCode.Pages;

internal sealed partial class FallbackWorkspaceItem : FallbackCommandItem
{
    private const string _id = "tanchekwei.WorkspaceLauncherForVSCode.fallback";

    private readonly VisualStudioCodePage _page;
    private readonly SettingsManager _settingsManager;
    private readonly WorkspaceStorage _workspaceStorage;
    private readonly CountTracker _countTracker;
    private readonly CommandContextItem _refreshWorkspacesCommandContextItem;
    private readonly IPinService _pinService;

    public FallbackWorkspaceItem(
        VisualStudioCodePage page,
        SettingsManager settingsManager,
        WorkspaceStorage workspaceStorage,
        CountTracker countTracker,
        CommandContextItem refreshWorkspacesCommandContextItem,
        IPinService pinService)
        : base(new NoOpCommand(), "Open Recent Visual Studio / Code", _id)
    {
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        _page = page;
        _settingsManager = settingsManager;
        _workspaceStorage = workspaceStorage;
        _countTracker = countTracker;
        _refreshWorkspacesCommandContextItem = refreshWorkspacesCommandContextItem;
        _pinService = pinService;
    }

    public override void UpdateQuery(string query)
    {
        Command = new NoOpCommand();
        Title = string.Empty;
        Subtitle = string.Empty;
        Icon = null;
        MoreCommands = null;

        if (string.IsNullOrWhiteSpace(query) || _page.AllWorkspaces.Count == 0)
        {
            return;
        }

        var filtered = WorkspaceFilter.Filter(query, _page.AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy);

        if (filtered.Count == 0)
        {
            return;
        }

        var best = filtered[0];
        var isVSSolution = best.WorkspaceType == WorkspaceType.Solution || best.WorkspaceType == WorkspaceType.Solution2026;

        List<VisualStudioInstance> vsInstances = _page.VSCodeService.GetVisualStudioInstances();

        var (command, icon, _, _, moreCommands) = isVSSolution
            ? WorkspaceItemFactory.CreateSolutionComponents(best, _page, _settingsManager, vsInstances)
            : WorkspaceItemFactory.CreateVSCodeComponents(best, _page, _settingsManager);

        WorkspaceItemFactory.AddCommonCommands(moreCommands, best, _settingsManager, _countTracker, _refreshWorkspacesCommandContextItem, _pinService);

        Title = best.Name ?? string.Empty;
        Subtitle = best.WindowsPath ?? string.Empty;
        Icon = icon;
        Command = command;
        MoreCommands = moreCommands.ToArray();
    }
}
