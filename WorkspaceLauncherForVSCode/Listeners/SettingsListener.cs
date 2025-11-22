// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Listeners
{
    public class SettingsListener
    {
        private readonly SettingsManager? _settingsManager;
        private VisualStudioCodeEdition _previousEditions;
        private bool _previousEnableVisualStudio;
        private SearchBy _previousSearchBy;
        private SecondaryCommand _previousVsSecondaryCommand;
        private SecondaryCommand _previousVscodeSecondaryCommand;
        private TagType _previousTagTypes;
        private SortBy _previousSortBy;
        private bool _prevEnableWorkspaceWatcher;
        private bool _previousUseHelperLauncher;

        public event EventHandler? InstanceSettingsChanged;
        public event EventHandler? PageSettingsChanged;
        public event EventHandler? SortSettingsChanged;

        public SettingsListener(SettingsManager settingsManager)
        {
            try
            {
                _settingsManager = settingsManager;
                _previousEditions = _settingsManager.EnabledEditions;
                _previousSearchBy = _settingsManager.SearchBy;
                _previousEnableVisualStudio = _settingsManager.EnableVisualStudio;
                _previousVsSecondaryCommand = _settingsManager.VSSecondaryCommand;
                _previousVscodeSecondaryCommand = _settingsManager.VSCodeSecondaryCommand;
                _previousTagTypes = _settingsManager.TagTypes;
                _previousSortBy = _settingsManager.SortBy;
                _prevEnableWorkspaceWatcher = _settingsManager.EnableWorkspaceWatcher;
                _previousUseHelperLauncher = _settingsManager.UseHelperLauncher;
                _settingsManager.Settings.SettingsChanged += OnSettingsChanged;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        private void OnSettingsChanged(object? sender, Settings e)
        {
            try
            {
                if (_settingsManager == null)
                {
                    return;
                }

                var currentEditions = _settingsManager.EnabledEditions;
                var currentSearchBy = _settingsManager.SearchBy;
                var currentEnableVisualStudio = _settingsManager.EnableVisualStudio;
                var currentVsSecondaryCommand = _settingsManager.VSSecondaryCommand;
                var currentVscodeSecondaryCommand = _settingsManager.VSCodeSecondaryCommand;
                var currentTagTypes = _settingsManager.TagTypes;
                var currentSortBy = _settingsManager.SortBy;
                var currentEnableWorkspaceWatcher = _settingsManager.EnableWorkspaceWatcher;
                var currentUseHelperLauncher = _settingsManager.UseHelperLauncher;

                if (currentEditions != _previousEditions || currentVsSecondaryCommand != _previousVsSecondaryCommand || currentVscodeSecondaryCommand != _previousVscodeSecondaryCommand || currentTagTypes != _previousTagTypes)
                {
                    InstanceSettingsChanged?.Invoke(this, EventArgs.Empty);
                    _previousEditions = currentEditions;
                    _previousVsSecondaryCommand = currentVsSecondaryCommand;
                    _previousVscodeSecondaryCommand = currentVscodeSecondaryCommand;
                    _previousTagTypes = currentTagTypes;
                }

                if (currentSearchBy != _previousSearchBy)
                {
                    PageSettingsChanged?.Invoke(this, EventArgs.Empty);
                    _previousSearchBy = currentSearchBy;
                }

                if (currentEnableWorkspaceWatcher != _prevEnableWorkspaceWatcher ||
                    currentSortBy != _previousSortBy)
                {
                    SortSettingsChanged?.Invoke(this, EventArgs.Empty);
                    _prevEnableWorkspaceWatcher = currentEnableWorkspaceWatcher;
                    _previousSortBy = currentSortBy;
                }

                if (currentEnableVisualStudio != _previousEnableVisualStudio)
                {
                    PageSettingsChanged?.Invoke(this, EventArgs.Empty);
                    InstanceSettingsChanged?.Invoke(this, EventArgs.Empty);
                    _previousEnableVisualStudio = currentEnableVisualStudio;
                }

                if (currentUseHelperLauncher != _previousUseHelperLauncher)
                {
                    _previousUseHelperLauncher = currentUseHelperLauncher;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }
    }
}