// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using OpenUrlCommand = WorkspaceLauncherForVSCode.Commands.OpenUrlCommand;

namespace WorkspaceLauncherForVSCode.Pages
{
    public static class StaticHelpItems
    {
        public static readonly ListItem OpenSettingsFolder = new(
            new OpenInExplorerCommand(Constant.SettingsFolderPath, null, "Open extension settings / logs folder")
        );

        public static readonly ListItem ReportBug = new(
            new OpenUrlCommand(
                $"https://github.com/tanchekwei/VisualStudioCodeForCommandPalette/issues/new?template=bug_report.yml&extension-version={Uri.EscapeDataString(Constant.AssemblyVersion)}",
                "Report issue",
                Icon.GitHub)
        );

        public static readonly ListItem ExtensionVersion = new()
        {
            Title = Constant.AssemblyVersion,
            Subtitle = "Extension Version",
            Icon = Icon.Extension
        };

        public static ListItem SettingsItem { get; private set; } = null!;
        public static ListItem ResetDatabase { get; private set; } = null!;
        public static ListItem ResetDatabaseItem => ResetDatabase;

        public static void Initialize(SettingsManager settingsManager)
        {
            Initialize(settingsManager, null, null);
        }

        public static void Initialize(SettingsManager settingsManager, WorkspaceStorage? workspaceStorage, Action? refreshAction)
        {
            try
            {
                SettingsItem = new ListItem(settingsManager.Settings.SettingsPage)
                {
                    Title = "Setting",
                    Icon = Icon.Setting
                };

                if (workspaceStorage != null && refreshAction != null)
                {
                    ResetDatabase = new ListItem(new ResetDatabaseCommand(workspaceStorage, refreshAction))
                    {
                        Title = "Reset Extension Database",
                        Subtitle = "Reset extension database and refresh all records",
                        Icon = Icon.EraseTool
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }
    }
}