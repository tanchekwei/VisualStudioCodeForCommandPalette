// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Services
{
    public partial class VSCodeWorkspaceWatcherService : IVSCodeWorkspaceWatcherService, IDisposable
    {
        private Timer? _timer;
        private readonly IVisualStudioCodeService _vscodeService;
        private readonly SettingsManager _settingsManager;
        private int _lastKnownVersion;
        private bool _isWatching;

        public event EventHandler? TriggerRefresh;

        public VSCodeWorkspaceWatcherService(IVisualStudioCodeService vscodeService, SettingsManager settingsManager)
        {
            _vscodeService = vscodeService;
            _settingsManager = settingsManager;
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
            if (_settingsManager.EnableWorkspaceWatcher && _settingsManager.SortBy == Enums.SortBy.RecentFromVSCode)
            {
                var instances = _vscodeService.GetInstances();
                if (instances.Count > 0)
                {
                    var instance = instances[0];
                    var dbPath = Path.Combine(instance.StoragePath, "state.vscdb");
                    if (File.Exists(dbPath))
                    {
                        VscdbRecentListChangeTrackerInitializer.Initialize(dbPath);
                    }
                }

                if (_timer == null)
                {
                    _timer = new Timer(async _ => await CheckForChanges(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
                }
                else
                {
                    _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(2));
                }
                _isWatching = true;
            }
        }

        public void StopWatching()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _isWatching = false;
        }

        private async Task CheckForChanges()
        {
#if DEBUG
                using var logger = new TimeLogger();
#endif
            var currentVersion = await GetCurrentVersionAsync();

            if (_lastKnownVersion == 0)
            {
                _lastKnownVersion = currentVersion;
                return;
            }

            if (currentVersion > _lastKnownVersion)
            {
                _lastKnownVersion = currentVersion;
                TriggerRefresh?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task<int> GetCurrentVersionAsync()
        {
            var totalVersion = 0;
            var instances = _vscodeService.GetInstances();
            if (instances.Count > 0)
            {
                var instance = instances[0];
                var dbPath = Path.Combine(instance.StoragePath, "state.vscdb");
                if (!File.Exists(dbPath))
                {
                    return 0;
                }

                try
                {
                    await using var connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly;");
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT version FROM CmdPalVisualStudioCodeHistoryRecentlyOpenedPathsListTracker";

                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        totalVersion += Convert.ToInt32(result);
                    }
                }
                catch (Exception)
                {
                }
            }
            return totalVersion;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}