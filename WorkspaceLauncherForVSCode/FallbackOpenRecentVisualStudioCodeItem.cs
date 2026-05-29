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
        Subtitle = Classes.Constant.VisualStudioCodeDisplayName;
        Icon = null;

        if (string.IsNullOrWhiteSpace(query) || _page.AllWorkspaces.Count == 0)
        {
            return;
        }

        Title = $"Search for \"{query}\"";
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        Command = new FallbackOpenRecentVisualStudioCodeCommand(_page, query);
    }
}

internal sealed partial class FallbackOpenRecentVisualStudioCodeCommand : InvokableCommand
{
    private readonly VisualStudioCodePage _page;
    private readonly string _query;

    public FallbackOpenRecentVisualStudioCodeCommand(VisualStudioCodePage page, string query)
    {
        _page = page;
        _query = query;
    }

    public override CommandResult Invoke()
    {
        _page.SetSearchText(_query);
        return CommandResult.GoToPage(new GoToPageArgs() { PageId = _page.Id });
    }
}