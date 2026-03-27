// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal sealed partial class SearchFilters : Filters
{
    public SearchFilters()
    {
        CurrentFilterId = "all";
    }

    public override IFilterItem[] GetFilters()
    {
        return [
            new Filter() { Id = nameof(FilterType.All), Name = "All", Icon = Icon.FilterIcon },
            new Separator(),
            new Filter() { Id = nameof(FilterType.Vscode), Name = "Visual Studio Code", Icon = Icon.VisualStudioCode },
            new Filter() { Id = nameof(FilterType.VscodeInsider), Name = "Visual Studio Code Insiders", Icon = Icon.VisualStudioCodeInsiders },
            new Filter() { Id = nameof(FilterType.Cursor), Name = "Cursor", Icon = Icon.Cursor },
            new Filter() { Id = nameof(FilterType.Antigravity), Name = "Antigravity", Icon = Icon.Antigravity },
            new Filter() { Id = nameof(FilterType.Windsurf), Name = "Windsurf", Icon = Icon.Windsurf },
            new Filter() { Id = nameof(FilterType.Vs), Name = "Visual Studio", Icon = Icon.VisualStudio2026 },
            new Separator(),
        ];
    }
}
