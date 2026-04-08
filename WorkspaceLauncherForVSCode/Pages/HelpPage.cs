// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Pages
{
    public sealed partial class HelpPage : ListPage
    {
        private readonly ListItem _settingsItem;
        private readonly VisualStudioCodeWorkspace? _workspace;
        public HelpPage(SettingsManager settingsManager, VisualStudioCodeWorkspace? workspace)
        {
            try
            {
                Name = "Help";
                Icon = Classes.Icon.Help;
                Id = "HelpPage";
                _settingsItem = new ListItem(settingsManager.Settings.SettingsPage) { Title = "Setting", Icon = Classes.Icon.Setting };
                _workspace = workspace;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                throw;
            }
        }

        public override IListItem[] GetItems()
        {
            try
            {
                var items = new List<IListItem>
                {
                    StaticHelpItems.ReportBug,
                    StaticHelpItems.ViewSource,
                };

                items.Add(StaticHelpItems.ExtensionVersion);
                items.Add(StaticHelpItems.SettingsItem);

                if (_workspace != null)
                    items.Add(new ListItem(new DetailPage(_workspace)));

                items.Add(StaticHelpItems.OpenSettingsFolder);

                return items.ToArray();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return [];
            }
        }
    }
}
