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
    private readonly int _index;

    public FallbackWorkspaceItem(
        VisualStudioCodePage page,
        int index)
        : base(new NoOpCommand(),
        $"{Constant.VisualStudioCodeDisplayName} fallback result no. {index + 1}",
        $"{Package.Current.Id.Name}.{nameof(FallbackWorkspaceItem)}.{index}")
    {
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        _page = page;
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

        var filtered = _page.GetFallbackFilteredWorkspaces(query);

        if (filtered.Count <= _index)
        {
            return;
        }

        var best = filtered[_index];
        var listItem = _page.GetOrCreateListItem(best);

        Title = listItem.Title;
        Subtitle = listItem.Subtitle;
        Icon = listItem.Icon;
        Command = listItem.Command;
        MoreCommands = listItem.MoreCommands;
    }
}
