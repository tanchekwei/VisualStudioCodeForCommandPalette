// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
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
            Name = "Visual Studio Detail";
            Icon = Classes.Icon.Help;
            Id = "VisualStudioDetailPage";
            _countTracker = deps.Get<CountTracker>();
            _visualStudioService = deps.Get<VisualStudioService>();
        }

        public override IListItem[] GetItems()
        {
            List<ListItem> instancesDetails = new();
            foreach (var instance in _visualStudioService.Instances!)
            {
                instancesDetails.Add(new()
                {
                    Title = instance.InstancePath,
                    Subtitle = "Instance Path",
                    Icon = Classes.Icon.VisualStudio,
                });
            }
            return [
                ..instancesDetails,
                new ListItem
                {
                    Title = _countTracker[CountType.VisualStudio].ToString(CultureInfo.InvariantCulture),
                    Subtitle = "Total",
                    Icon = Classes.Icon.VisualStudio,
                }
            ];
        }
    }
}
