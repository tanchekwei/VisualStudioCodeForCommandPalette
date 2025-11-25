// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;

namespace WorkspaceLauncherForVSCode.Pages
{
    public static class StaticHelpItems
    {
        public static readonly ListItem OpenSettingsFolder = new(
            new OpenInExplorerCommand(Constant.SettingsFolderPath, null, "Open extension settings / logs folder")
        );

        public static readonly ListItem ViewSource = new(
            new Commands.OpenUrlCommand("https://github.com/tanchekwei/VisualStudioCodeForCommandPalette", "View source code", Icon.GitHub)
        );

        public static readonly ListItem ReportBug = new(
            new Commands.OpenUrlCommand("https://github.com/tanchekwei/VisualStudioCodeForCommandPalette/issues/new", "Report issue", Icon.GitHub)
        );

        public static readonly ListItem ExtensionVersion = new()
        {
            Title = Constant.AssemblyVersion,
            Subtitle = "Extension Version",
            Icon = Icon.Extension
        };

        public static readonly List<ListItem> CountDetailItems = new()
        {
            new ListItem
            {
                Subtitle = "Folder",
                Icon = Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Workspace",
                Icon = Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Codespaces",
                Icon = Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "WSL",
                Icon = Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Dev Container",
                Icon = Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Attached Container",
                Icon = Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "SSH Remote",
                Icon = Icon.VisualStudioCode,
            },
            new ListItem
            {
                Subtitle = "Total",
                Icon = Icon.VisualStudioCode,
            }
        };

        public static ListItem SettingsItem { get; private set; } = null!;
        public static CommandContextItem VisualStudioCodeDetailPage { get; private set; } = null!;
        public static CommandContextItem VisualStudioDetailPage { get; private set; } = null!;
        public static List<ListItem> CountItems { get; private set; } = null!;
        public static void Initialize(Dependencies deps)
        {
            try
            {
                SettingsItem = new ListItem(deps.Get<SettingsManager>().Settings.SettingsPage)
                {
                    Title = "Setting",
                    Icon = Icon.Setting
                };
                VisualStudioCodeDetailPage = new(new VisualStudioCodeDetailPage(deps));
                VisualStudioDetailPage = new(new VisualStudioDetailPage(deps));
                CountItems = new()
                {
                    new ListItem(VisualStudioDetailPage)
                    {
                        Subtitle = "Visual Studio",
                        Icon = Icon.VisualStudio2026,
                    },
                    new ListItem(VisualStudioCodeDetailPage)
                    {
                        Subtitle = "Visual Studio Code",
                        Icon = Icon.VisualStudioCode,
                    },
                    new ListItem()
                    {
                        Subtitle = "Total",
                        Icon = Icon.VisualStudioAndVisualStudioCode,
                    }
                };
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }
    }
}
