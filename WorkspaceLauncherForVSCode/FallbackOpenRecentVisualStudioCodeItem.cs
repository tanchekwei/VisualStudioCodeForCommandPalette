// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel;

namespace WorkspaceLauncherForVSCode.Pages;

internal sealed partial class FallbackOpenRecentVisualStudioCodeItem : FallbackCommandItem
{
    private readonly VisualStudioCodePage _page;

    public FallbackOpenRecentVisualStudioCodeItem(
        VisualStudioCodePage page
    )
        : base(new NoOpCommand(), string.Empty, $"{Package.Current.Id.Name}.{nameof(FallbackOpenRecentVisualStudioCodeItem)}")
    {
        _page = page;
        Title = string.Empty;
        Subtitle = string.Empty;
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
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

        _page.SetSearchText(query);
        var displayName = Classes.Constant.VisualStudioCodeDisplayName;
        Title = $"Search for \"{query}\" in {displayName}";
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        Command = _page;
    }
}
