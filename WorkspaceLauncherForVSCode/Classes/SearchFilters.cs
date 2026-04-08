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
        CurrentFilterId = nameof(FilterType.All);
    }

    public override IFilterItem[] GetFilters()
    {
        return [
            new Filter() { Id = nameof(FilterType.All), Name = "All", Icon = Icon.FilterIcon },
            new Separator(),
            new Filter() { Id = nameof(FilterType.VisualStudio2026), Name = "Visual Studio 2026", Icon = Icon.VisualStudio2026 },
            new Filter() { Id = nameof(FilterType.VisualStudio), Name = "Visual Studio", Icon = Icon.VisualStudio },
            new Separator(),
            new Filter() { Id = nameof(FilterType.Vscode), Name = "Visual Studio Code", Icon = Icon.VisualStudioCode },
            new Filter() { Id = nameof(FilterType.VscodeInsider), Name = "Visual Studio Code Insiders", Icon = Icon.VisualStudioCodeInsiders },
            new Separator("Workspaces"),
            new Filter() { Id = nameof(FilterType.Folder), Name = "Folder", Icon = Icon.VisualStudioCode },
            new Filter() { Id = nameof(FilterType.Workspace), Name = "Workspace", Icon = Icon.VisualStudioCode },
            new Separator("Remote Environments"),
            new Filter() { Id = nameof(FilterType.RemoteWsl), Name = "WSL", Icon = Icon.Web },
            new Filter() { Id = nameof(FilterType.RemoteDevContainer), Name = "Dev Container", Icon = Icon.Web },
            new Filter() { Id = nameof(FilterType.RemoteCodespaces), Name = "Codespaces", Icon = Icon.Web },
            new Filter() { Id = nameof(FilterType.RemoteAttachedContainer), Name = "Attached Container", Icon = Icon.Web },
            new Filter() { Id = nameof(FilterType.RemoteSSHRemote), Name = "SSH Remote", Icon = Icon.Web },
            new Separator("AI Editors (Visual Studio Code Forks)"),
            new Filter() { Id = nameof(FilterType.Antigravity), Name = "Antigravity", Icon = Icon.Antigravity },
            new Filter() { Id = nameof(FilterType.Cursor), Name = "Cursor", Icon = Icon.Cursor },
            new Filter() { Id = nameof(FilterType.Windsurf), Name = "Windsurf", Icon = Icon.Windsurf },
            new Separator(),
        ];
    }
}
