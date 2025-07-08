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
        private string _previousPreferredEdition;
        private bool _previousShowDetails;
        private bool _previousEnableVisualStudio;
        private SearchBy _previousSearchBy;
        private SecondaryCommand _previousVsSecondaryCommand;
        private SecondaryCommand _previousVscodeSecondaryCommand;

        public event EventHandler? InstanceSettingsChanged;
        public event EventHandler? PageSettingsChanged;

        public SettingsListener(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _previousEditions = _settingsManager.EnabledEditions;
            _previousPreferredEdition = _settingsManager.PreferredEdition;
            _previousSearchBy = _settingsManager.SearchBy;
            _previousEnableVisualStudio = _settingsManager.EnableVisualStudio;
            _previousVsSecondaryCommand = _settingsManager.VSSecondaryCommand;
            _previousVscodeSecondaryCommand = _settingsManager.VSCodeSecondaryCommand;
            _settingsManager.Settings.SettingsChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object? sender, Settings e)
        {
            var currentEditions = _settingsManager.EnabledEditions;
            var currentPreferredEdition = _settingsManager.PreferredEdition;
            var currentSearchBy = _settingsManager.SearchBy;
            var currentEnableVisualStudio = _settingsManager.EnableVisualStudio;
            var currentVsSecondaryCommand = _settingsManager.VSSecondaryCommand;
            var currentVscodeSecondaryCommand = _settingsManager.VSCodeSecondaryCommand;

            if (currentEditions != _previousEditions || currentPreferredEdition != _previousPreferredEdition || currentVsSecondaryCommand != _previousVsSecondaryCommand || currentVscodeSecondaryCommand != _previousVscodeSecondaryCommand)
            {
                InstanceSettingsChanged?.Invoke(this, EventArgs.Empty);
                _previousEditions = currentEditions;
                _previousPreferredEdition = currentPreferredEdition;
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