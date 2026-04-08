// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Workspaces;

namespace WorkspaceLauncherForVSCode.Pages;

internal sealed partial class FallbackWorkspaceItem : FallbackCommandItem
{
    private readonly VisualStudioCodePage _page;
    private readonly SettingsManager _settingsManager;
    private readonly WorkspaceStorage _workspaceStorage;
    private readonly CountTracker _countTracker;
    private readonly CommandContextItem _refreshWorkspacesCommandContextItem;
    private readonly IPinService _pinService;
    private readonly int _index;

    public FallbackWorkspaceItem(
        VisualStudioCodePage page,
        SettingsManager settingsManager,
        WorkspaceStorage workspaceStorage,
        CountTracker countTracker,
        CommandContextItem refreshWorkspacesCommandContextItem,
        IPinService pinService,
        int index)
        : base(new NoOpCommand(),
#if DEBUG
        "Open Recent Visual Studio / Code (Dev)",
#else
        "Open Recent Visual Studio / Code",
#endif
        $"{Package.Current.Id.Name}.{nameof(FallbackWorkspaceItem)}.{index}")
    {
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        _page = page;
        _settingsManager = settingsManager;
        _workspaceStorage = workspaceStorage;
        _countTracker = countTracker;
        _refreshWorkspacesCommandContextItem = refreshWorkspacesCommandContextItem;
        _pinService = pinService;
        _index = index;
    }

    public override void UpdateQuery(string query)
    {
        Command = new NoOpCommand();
        Title = string.Empty;
        Subtitle = string.Empty;
        Icon = null;

        if (string.IsNullOrWhiteSpace(query) || _page.AllWorkspaces.Count == 0)
        {
            return;
        }

        var filtered = WorkspaceFilter.Filter(query, _page.AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy, string.Empty);

        if (filtered.Count <= _index)
        {
            return;
        }

        var best = filtered[_index];
        var isVSSolution = best.WorkspaceType == WorkspaceType.Solution || best.WorkspaceType == WorkspaceType.Solution2026;
        var (command, icon, _, _, moreCommands) = isVSSolution
            ? WorkspaceItemFactory.CreateSolutionComponents(best, _page, _settingsManager, _page.VSCodeService.GetVisualStudioInstances())
            : WorkspaceItemFactory.CreateVSCodeComponents(best, _page, _settingsManager);

        WorkspaceItemFactory.AddCommonCommands(moreCommands, best, _settingsManager, _countTracker, _refreshWorkspacesCommandContextItem, _pinService);

        Title = best.Name ?? string.Empty;
        Subtitle = best.WindowsPath ?? string.Empty;
        Icon = icon;
        Command = command;
        MoreCommands = moreCommands.ToArray();
    }
}
