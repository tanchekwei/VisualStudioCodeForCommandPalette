// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Pages
{
    public sealed partial class HelpPage : ListPage
    {
        private readonly ListItem _settingsItem;
        private readonly CountTracker _countTracker;
        private readonly VisualStudioCodeWorkspace? _workspace;
        public HelpPage(SettingsManager settingsManager, CountTracker countTracker, VisualStudioCodeWorkspace? workspace)
        {
            Name = "Help";
            Icon = Classes.Icon.Help;
            Id = "HelpPage";
            _settingsItem = new ListItem(settingsManager.Settings.SettingsPage) { Title = "Setting", Icon = Classes.Icon.Setting };
            _countTracker = countTracker;
            _workspace = workspace;
        }

        public override IListItem[] GetItems()
        {
            StaticHelpItems.CountItems[0].Title = _countTracker[CountType.VisualStudio].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountItems[1].Title = _countTracker[CountType.VisualStudioCode].ToString(CultureInfo.InvariantCulture);
            StaticHelpItems.CountItems[2].Title = _countTracker[CountType.Total].ToString(CultureInfo.InvariantCulture);

            var items = new List<IListItem>
            {
                StaticHelpItems.ReportBug,
                StaticHelpItems.ViewSource,
            };

            items.AddRange(StaticHelpItems.CountItems);

            items.Add(StaticHelpItems.ExtensionVersion);
            items.Add(StaticHelpItems.SettingsItem);

            if (_workspace != null)
                items.Add(new ListItem(new DetailPage(_workspace)));

            items.Add(StaticHelpItems.OpenSettingsFolder);

            return items.ToArray();
        }
    }
}
