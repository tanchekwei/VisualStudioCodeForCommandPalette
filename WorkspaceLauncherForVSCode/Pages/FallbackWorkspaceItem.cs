// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Pages;

internal sealed partial class FallbackWorkspaceItem : FallbackCommandItem
{
    private static readonly NoOpCommand s_noOpCommand = new();
    private readonly VisualStudioCodePage _page;
    private readonly int _index;
    private string? _lastQuery;

    public FallbackWorkspaceItem(
        VisualStudioCodePage page,
        int index)
        : base(s_noOpCommand,
        $"{Constant.VisualStudioCodeDisplayName} fallback result no. {index + 1}",
        $"{Package.Current.Id.Name}.{nameof(FallbackWorkspaceItem)}.{index}")
    {
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        _page = page;
        _index = index;
    }

    public override void UpdateQuery(string query)
    {
        if (string.Equals(_lastQuery, query, StringComparison.Ordinal))
        {
            return;
        }

        _lastQuery = query;

        if (string.IsNullOrWhiteSpace(query) || _page.AllWorkspaces.Count == 0)
        {
            Command = s_noOpCommand;
            Title = string.Empty;
            Subtitle = string.Empty;
            Icon = null;
            return;
        }

        var filtered = _page.GetFallbackFilteredWorkspaces(query);

        if (filtered.Count <= _index)
        {
            Command = s_noOpCommand;
            Title = string.Empty;
            Subtitle = string.Empty;
            Icon = null;
            return;
        }

        var best = filtered[_index];
        var listItem = _page.GetOrCreateListItem(best, true);

        Title = listItem.Title;
        Subtitle = listItem.Subtitle;
        Icon = listItem.Icon;
        Command = listItem.Command;
        MoreCommands = listItem.MoreCommands;
    }
}