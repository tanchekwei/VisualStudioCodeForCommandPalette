// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Reflection;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;

using OpenUrlCommand = WorkspaceLauncherForVSCode.Commands.OpenUrlCommand;

namespace WorkspaceLauncherForVSCode.Pages
{
    public sealed partial class HelpPage : ListPage
    {
        private readonly ListItem _settingsItem;
        private readonly CountTracker _countTracker;
        public HelpPage(SettingsManager settingsManager, CountTracker countTracker)
        {
            Name = "Help";
            Icon = Classes.Icon.Help;
            Id = "HelpPage";
            _settingsItem = new ListItem(settingsManager.Settings.SettingsPage) { Title = "Setting", Icon = Classes.Icon.Setting };
            _countTracker = countTracker;
        }

        public override IListItem[] GetItems()
        {
            StaticHelpItems.CountItems[0].Title = _countTracker[CountType.VisualStudio].ToString();
            StaticHelpItems.CountItems[1].Title = _countTracker[CountType.VisualStudioCode].ToString();
            StaticHelpItems.CountItems[2].Title = _countTracker[CountType.Total].ToString();
            return [
                StaticHelpItems.ReportBug,
                StaticHelpItems.ViewSource,
                ..StaticHelpItems.CountItems,
                StaticHelpItems.ExtensionVersion,
                StaticHelpItems.SettingsItem,
                StaticHelpItems.OpenSettings,
            ];
        }
    }
}
