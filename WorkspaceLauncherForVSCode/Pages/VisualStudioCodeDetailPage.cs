// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Services.VisualStudio;

namespace WorkspaceLauncherForVSCode.Pages
{
    public sealed partial class VisualStudioCodeDetailPage : ListPage
    {
        private readonly IVisualStudioCodeService _visualStudioCodeService;
        public VisualStudioCodeDetailPage(Dependencies deps)
        {
            Name = "Visual Studio Code Detail";
            Icon = Classes.Icon.Help;
            Id = "VisualStudioCodeDetailPage";
            _visualStudioCodeService = deps.Get<IVisualStudioCodeService>();
        }

        public override IListItem[] GetItems()
        {
            try
            {
                List<ListItem> instancesDetails = new();
                foreach (var instance in _visualStudioCodeService.GetInstances())
                {
                    instancesDetails.Add(new()
                    {
                        Title = instance.ExecutablePath,
                        Subtitle = "Instance Path",
                        Icon = Classes.Icon.VisualStudioCode,
                    });
                }
                return instancesDetails.ToArray();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return [];
            }
        }
    }
}
