// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal sealed partial class SearchFilters : Filters
{
    private readonly SettingsManager _settingsManager;

    public SearchFilters(SettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        CurrentFilterId = nameof(FilterType.All);
        PropChanged += SearchFilters_PropChanged;
    }

    private void SearchFilters_PropChanged(object sender, IPropChangedEventArgs args)
    {
        var filterId = CurrentFilterId;
        var editions = _settingsManager.EnabledEditions;

        if (filterId == nameof(FilterType.VisualStudio) || filterId == nameof(FilterType.VisualStudio2026))
        {
            if (!_settingsManager.EnableVisualStudio)
            {
                new ToastStatusMessage("Visual Studio is disabled in settings").Show();
            }
        }
        else if (filterId == nameof(FilterType.Vscode))
        {
            if (!editions.HasFlag(VisualStudioCodeEdition.Default) &&
                !editions.HasFlag(VisualStudioCodeEdition.System) &&
                !editions.HasFlag(VisualStudioCodeEdition.Custom) &&
                !editions.HasFlag(VisualStudioCodeEdition.CustomPath))
            {
                new ToastStatusMessage("Visual Studio Code is disabled in settings").Show();
            }
        }
        else if (filterId == nameof(FilterType.VscodeInsider))
        {
            if (!editions.HasFlag(VisualStudioCodeEdition.Insider))
            {
                new ToastStatusMessage("Visual Studio Code Insiders is disabled in settings").Show();
            }
        }
        else if (filterId == nameof(FilterType.Cursor))
        {
            if (!editions.HasFlag(VisualStudioCodeEdition.Cursor))
            {
                new ToastStatusMessage("Cursor is disabled in settings").Show();
            }
        }
        else if (filterId == nameof(FilterType.Antigravity))
        {
            if (!editions.HasFlag(VisualStudioCodeEdition.Antigravity))
            {
                new ToastStatusMessage("Antigravity is disabled in settings").Show();
            }
        }
        else if (filterId == nameof(FilterType.Windsurf))
        {
            if (!editions.HasFlag(VisualStudioCodeEdition.Windsurf))
            {
                new ToastStatusMessage("Windsurf is disabled in settings").Show();
            }
        }
        else if (filterId == nameof(FilterType.Vscodium))
        {
            if (!editions.HasFlag(VisualStudioCodeEdition.Vscodium))
            {
                new ToastStatusMessage("VSCodium is disabled in settings").Show();
            }
        }
        else if (filterId == nameof(FilterType.Folder) ||
                 filterId == nameof(FilterType.Workspace) ||
                 filterId == nameof(FilterType.RemoteWsl) ||
                 filterId == nameof(FilterType.RemoteDevContainer) ||
                 filterId == nameof(FilterType.RemoteCodespaces) ||
                 filterId == nameof(FilterType.RemoteAttachedContainer) ||
                 filterId == nameof(FilterType.RemoteSSHRemote))
        {
            if (editions == VisualStudioCodeEdition.None)
            {
                new ToastStatusMessage("Visual Studio Code and AI Editors are disabled in settings").Show();
            }
        }
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
            new Separator("Visual Studio Code Forks"),
            new Filter() { Id = nameof(FilterType.Vscodium), Name = "VSCodium", Icon = Icon.Vscodium },
            new Filter() { Id = nameof(FilterType.Antigravity), Name = "Antigravity", Icon = Icon.Antigravity },
            new Filter() { Id = nameof(FilterType.Cursor), Name = "Cursor", Icon = Icon.Cursor },
            new Filter() { Id = nameof(FilterType.Windsurf), Name = "Windsurf", Icon = Icon.Windsurf },
            new Separator(),
        ];
    }
}
