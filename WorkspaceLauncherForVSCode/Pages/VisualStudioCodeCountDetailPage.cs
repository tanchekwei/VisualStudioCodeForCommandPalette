// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Globalization;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Pages
{
    public sealed partial class VisualStudioCodeCountDetailPage : ListPage
    {
        private readonly CountTracker _countTracker;
        private readonly VisualStudioCodePage _page;
        public VisualStudioCodeCountDetailPage(Dependencies deps)
        {
            Name = "Visual Studio Code Count Detail";
            Icon = Classes.Icon.Help;
            Id = "VisualStudioCodeCountDetailPage";
            _countTracker = deps.Get<CountTracker>();
            _page = deps.Get<VisualStudioCodePage>();
        }

        public override IListItem[] GetItems()
        {
            _countTracker.Reset();
            foreach (var workspace in _page.AllWorkspaces)
            {
                if (workspace is null)
                {
                    continue;
                }
                _countTracker.Increment(CountType.Total);
                switch (workspace.WorkspaceType)
                {
                    case WorkspaceType.Workspace:
                    case WorkspaceType.Folder:
                        _countTracker.Increment(CountType.VisualStudioCode);
                        if (workspace?.VisualStudioCodeRemoteUri?.Type != null)
                        {
                            _countTracker.Increment((VisualStudioCodeRemoteType)workspace.VisualStudioCodeRemoteUri.Type);
                        }
                        else
                        {
                            _countTracker.Increment(workspace.WorkspaceType);
                        }
                        break;
                    case WorkspaceType.Solution:
                        _countTracker.Increment(WorkspaceType.Solution);
                        _countTracker.Increment(CountType.VisualStudio);
                        break;
                    default:
                        break;
                }
            }

            StaticHelpItems.CountDetailItems[0].Title = _countTracker[WorkspaceType.Folder].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountDetailItems[1].Title = _countTracker[WorkspaceType.Workspace].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountDetailItems[2].Title = _countTracker[VisualStudioCodeRemoteType.Codespaces].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountDetailItems[3].Title = _countTracker[VisualStudioCodeRemoteType.WSL].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountDetailItems[4].Title = _countTracker[VisualStudioCodeRemoteType.DevContainer].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountDetailItems[5].Title = _countTracker[VisualStudioCodeRemoteType.AttachedContainer].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountDetailItems[6].Title = _countTracker[VisualStudioCodeRemoteType.SSHRemote].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountDetailItems[7].Title = _countTracker[CountType.VisualStudioCode].ToString(CultureInfo.InvariantCulture);
            return StaticHelpItems.CountDetailItems.ToArray();
        }
    }
}
