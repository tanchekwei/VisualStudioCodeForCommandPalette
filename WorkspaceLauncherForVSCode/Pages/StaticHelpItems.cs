// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;

namespace WorkspaceLauncherForVSCode.Pages
{
    public static class StaticHelpItems
    {
        public static readonly ListItem OpenSettings = new(
            new OpenInExplorerCommand(Utilities.BaseSettingsPath(Constant.AppName), null, "Open extension settings / logs folder")
        );

        public static readonly ListItem ViewSource = new(
            new Commands.OpenUrlCommand("https://github.com/tanchekwei/WorkspaceLauncherForVSCode", "View source code", Classes.Icon.GitHub)
        );

        public static readonly ListItem ReportBug = new(
            new Commands.OpenUrlCommand("https://github.com/tanchekwei/WorkspaceLauncherForVSCode/issues/new", "Report issue", Classes.Icon.GitHub)
        );

        public static readonly ListItem ExtensionVersion = new()
        {
            Title = Constant.AssemblyVersion,
            Subtitle = "Extension Version",
            Icon = Classes.Icon.Extension
        };

        public static readonly List<ListItem> CountItems = new()
        {
            new ListItem
            {
                Subtitle = "Visual Studio Count",
                Icon = Classes.Icon.VisualStudio,
            },
            new ListItem
            {
                Subtitle = "Visual Studio Code Count",
                Icon = Classes.Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Visual Studio / Code Count",
                Icon = Classes.Icon.VisualStudioAndVisualStudioCode,
            }
        };

        public static ListItem SettingsItem { get; private set; } = null!;
        public static void Initialize(SettingsManager settingsManager)
        {
            SettingsItem = new ListItem(settingsManager.Settings.SettingsPage)
            {
                Title = "Setting",
                Icon = Classes.Icon.Setting
            };
        }
    }
}
