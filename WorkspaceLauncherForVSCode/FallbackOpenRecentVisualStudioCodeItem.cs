// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel;

namespace WorkspaceLauncherForVSCode.Pages;

internal sealed partial class FallbackOpenRecentVisualStudioCodeItem : FallbackCommandItem
{
    private static readonly NoOpCommand s_noOpCommand = new();
    private readonly VisualStudioCodePage _page;
    private string? _lastQuery;

    public FallbackOpenRecentVisualStudioCodeItem(
        VisualStudioCodePage page
    )
        : base(s_noOpCommand, string.Empty, $"{Package.Current.Id.Name}.{nameof(FallbackOpenRecentVisualStudioCodeItem)}")
    {
        _page = page;
        Title = string.Empty;
        Subtitle = string.Empty;
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
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
            Subtitle = Classes.Constant.VisualStudioCodeDisplayName;
            Icon = null;
            return;
        }

        _page.SetSearchText(query);
        Title = $"Search for \"{query}\"";
        Subtitle = Classes.Constant.VisualStudioCodeDisplayName;
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        Command = _page;
    }
}