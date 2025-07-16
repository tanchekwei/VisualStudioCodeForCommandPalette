// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Listeners
{
    public class SettingsListener
    {
        private readonly SettingsManager _settingsManager;
        private VisualStudioCodeEdition _previousEditions;
        private bool _previousEnableVisualStudio;
        private SearchBy _previousSearchBy;
        private SecondaryCommand _previousVsSecondaryCommand;
        private SecondaryCommand _previousVscodeSecondaryCommand;
        private TagType _previousTagTypes;
        private SortBy _previousSortBy;

        public event EventHandler? InstanceSettingsChanged;
        public event EventHandler? PageSettingsChanged;

        public SettingsListener(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _previousEditions = _settingsManager.EnabledEditions;
            _previousSearchBy = _settingsManager.SearchBy;
            _previousEnableVisualStudio = _settingsManager.EnableVisualStudio;
            _previousVsSecondaryCommand = _settingsManager.VSSecondaryCommand;
            _previousVscodeSecondaryCommand = _settingsManager.VSCodeSecondaryCommand;
            _previousTagTypes = _settingsManager.TagTypes;
            _previousSortBy = _settingsManager.SortBy;
            _settingsManager.Settings.SettingsChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object? sender, Settings e)
        {
            var currentEditions = _settingsManager.EnabledEditions;
            var currentSearchBy = _settingsManager.SearchBy;
            var currentEnableVisualStudio = _settingsManager.EnableVisualStudio;
            var currentVsSecondaryCommand = _settingsManager.VSSecondaryCommand;
            var currentVscodeSecondaryCommand = _settingsManager.VSCodeSecondaryCommand;
            var currentTagTypes = _settingsManager.TagTypes;
            var currentSortBy = _settingsManager.SortBy;

            if (currentEditions != _previousEditions || currentVsSecondaryCommand != _previousVsSecondaryCommand || currentVscodeSecondaryCommand != _previousVscodeSecondaryCommand || currentTagTypes != _previousTagTypes || currentSortBy != _previousSortBy)
            {
                InstanceSettingsChanged?.Invoke(this, EventArgs.Empty);
                _previousEditions = currentEditions;
                _previousVsSecondaryCommand = currentVsSecondaryCommand;
                _previousVscodeSecondaryCommand = currentVscodeSecondaryCommand;
                _previousTagTypes = currentTagTypes;
                _previousSortBy = currentSortBy;
            }

            if (currentSearchBy != _previousSearchBy)
            {
                PageSettingsChanged?.Invoke(this, EventArgs.Empty);
                _previousSearchBy = currentSearchBy;
            }

            if (currentEnableVisualStudio != _previousEnableVisualStudio)
            {
                PageSettingsChanged?.Invoke(this, EventArgs.Empty);
                InstanceSettingsChanged?.Invoke(this, EventArgs.Empty);
                _previousEnableVisualStudio = currentEnableVisualStudio;
            }
        }
    }
}