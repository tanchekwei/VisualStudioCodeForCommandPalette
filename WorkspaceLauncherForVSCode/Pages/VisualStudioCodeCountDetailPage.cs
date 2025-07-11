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
        public VisualStudioCodeCountDetailPage(CountTracker countTracker)
        {
            Name = "Visual Studio Code Count Detail";
            Icon = Classes.Icon.Help;
            Id = "VisualStudioCodeCountDetailPage";
            _countTracker = countTracker;
        }

        public override IListItem[] GetItems()
        {
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
