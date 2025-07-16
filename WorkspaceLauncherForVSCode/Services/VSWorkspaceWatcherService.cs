// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Services.VisualStudio;

namespace WorkspaceLauncherForVSCode.Services
{
    public partial class VSWorkspaceWatcherService : IVSWorkspaceWatcherService, IDisposable
    {
        private readonly List<FileSystemWatcher> _watchers = new();
        private readonly SettingsManager _settingsManager;
        private readonly VisualStudioService _visualStudioService;
        private bool _isWatching;

        public event EventHandler? TriggerRefresh;

        public VSWorkspaceWatcherService(SettingsManager settingsManager, VisualStudioService visualStudioService)
        {
            _settingsManager = settingsManager;
            _visualStudioService = visualStudioService;
        }

        public void StartWatching()
        {
            if (_isWatching)
            {
                return;
            }
#if DEBUG
            using var logger = new TimeLogger();
#endif
            if (_settingsManager.EnableWorkspaceWatcher && _settingsManager.SortBy == Enums.SortBy.RecentFromVS)
            {
                _visualStudioService.InitInstances(Array.Empty<string>());
                var instances = _visualStudioService.Instances;
                if (instances is null)
                {
                    return;
                }
                foreach (var instance in instances)
                {
                    if (instance.ApplicationPrivateSettingsPath is not null && File.Exists(instance.ApplicationPrivateSettingsPath))
                    {
                        var watcher = new FileSystemWatcher
                        {
                            Path = Path.GetDirectoryName(instance.ApplicationPrivateSettingsPath)!,
                            Filter = Path.GetFileName(instance.ApplicationPrivateSettingsPath),
                            NotifyFilter = NotifyFilters.LastWrite,
                            EnableRaisingEvents = true
                        };
                        watcher.Changed += OnChanged;
                        _watchers.Add(watcher);
                    }
                }
                _isWatching = true;
            }
        }

        public void StopWatching()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnChanged;
                watcher.Dispose();
            }
            _watchers.Clear();
            _isWatching = false;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            TriggerRefresh?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            StopWatching();
            GC.SuppressFinalize(this);
        }
    }
}