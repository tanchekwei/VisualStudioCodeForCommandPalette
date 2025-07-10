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
        public static readonly ListItem OpenSettingsFolder = new(
            new OpenInExplorerCommand(Utilities.BaseSettingsPath(Constant.AppName), null, "Open extension settings / logs folder")
        );

        public static readonly ListItem ViewSource = new(
            new Commands.OpenUrlCommand("https://github.com/tanchekwei/VisualStudioCodeForCommandPalette", "View source code", Classes.Icon.GitHub)
        );

        public static readonly ListItem ReportBug = new(
            new Commands.OpenUrlCommand("https://github.com/tanchekwei/VisualStudioCodeForCommandPalette/issues/new", "Report issue", Classes.Icon.GitHub)
        );

        public static readonly ListItem ExtensionVersion = new()
        {
            Title = Constant.AssemblyVersion,
            Subtitle = "Extension Version",
            Icon = Classes.Icon.Extension
        };

        public static List<ListItem> CountItems { get; private set; } = null!;
        public static CommandContextItem CountDetail { get; private set; } = null!;

        public static readonly List<ListItem> CountDetailItems = new()
        {
            new ListItem
            {
                Subtitle = "Folder",
                Icon = Classes.Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Workspace",
                Icon = Classes.Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Codespaces",
                Icon = Classes.Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "WSL",
                Icon = Classes.Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Dev Container",
                Icon = Classes.Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Attached Container",
                Icon = Classes.Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "SSH Remote",
                Icon = Classes.Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Total",
                Icon = Classes.Icon.VisualStudioCode,
            }
        };

        public static ListItem SettingsItem { get; private set; } = null!;
        public static void Initialize(SettingsManager settingsManager, CountTracker countTracker)
        {
            SettingsItem = new ListItem(settingsManager.Settings.SettingsPage)
            {
                Title = "Setting",
                Icon = Classes.Icon.Setting
            };
            CountDetail = new(new VisualStudioCodeCountDetailPage(countTracker));
            CountItems = new()
            {
                new ListItem
                {
                    Subtitle = "Visual Studio",
                    Icon = Classes.Icon.VisualStudio,
                },
                new ListItem(CountDetail)
                {
                    Subtitle = "Visual Studio Code",
                    Icon = Classes.Icon.VisualStudioCode,
                },
                new ListItem
                {
                    Subtitle = "Total",
                    Icon = Classes.Icon.VisualStudioAndVisualStudioCode,
                }
            };
        }
    }
}
