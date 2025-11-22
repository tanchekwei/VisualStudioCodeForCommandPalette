// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Services;
using WorkspaceLauncherForVSCode.Services.VisualStudio;

namespace WorkspaceLauncherForVSCode.Pages
{
    public sealed partial class VisualStudioDetailPage : ListPage
    {
        private readonly CountTracker _countTracker;
        private readonly VisualStudioService _visualStudioService;
        public VisualStudioDetailPage(Dependencies deps)
        {
            try
            {
                Name = "Visual Studio Detail";
                Icon = Classes.Icon.Help;
                Id = "VisualStudioDetailPage";
                _countTracker = deps.Get<CountTracker>();
                _visualStudioService = deps.Get<VisualStudioService>();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                _countTracker = null!;
                _visualStudioService = null!;
                throw;
            }
        }

        public override IListItem[] GetItems()
        {
            try
            {
                List<ListItem> instancesDetails = new();
                if (_visualStudioService.Instances != null)
                {
                    foreach (var instance in _visualStudioService.Instances)
                    {
                        var icon = Classes.Icon.VisualStudio;
                        if (instance.ProductLineVersion == Constant.VisualStudio2026Version)
                        {
                            icon = Classes.Icon.VisualStudio2026;
                        }

                        instancesDetails.Add(new()
                        {
                            Title = instance.InstancePath,
                            Subtitle = "Instance Path",
                            Icon = icon,
                        });
                    }
                }
                return [
                    .. instancesDetails,
                    new ListItem
                    {
                        Title = _countTracker[CountType.VisualStudio].ToString(CultureInfo.InvariantCulture),
                        Subtitle = "Total",
                        Icon = Classes.Icon.VisualStudio,
                    }
                ];
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return [];
            }
        }
    }
}
