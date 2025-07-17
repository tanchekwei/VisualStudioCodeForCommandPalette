// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Data;
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
        private SqliteConnection? _cachedConnection;
        private SqliteCommand? _cachedCommand;
        private string? _cachedDbPath;

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
                    _cachedDbPath = Path.Combine(instance.StoragePath, "state.vscdb");
                    if (File.Exists(_cachedDbPath))
                    {
                        VscdbRecentListChangeTrackerInitializer.Initialize(_cachedDbPath);
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
            CloseConnection();
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
            if (string.IsNullOrEmpty(_cachedDbPath) || !File.Exists(_cachedDbPath))
            {
                return 0;
            }

            try
            {
                await EnsureConnectionAndCommandAsync(_cachedDbPath);

                if (_cachedCommand == null)
                {
                    return 0;
                }

                var result = await _cachedCommand.ExecuteScalarAsync();
                if (result is long l)
                {
                    return (int)l;
                }
            }
            catch (Exception)
            {
                CloseConnection();
            }

            return 0;
        }

        private async Task EnsureConnectionAndCommandAsync(string dbPath)
        {
            if (_cachedConnection == null || _cachedConnection.State != ConnectionState.Open)
            {
                CloseConnection(); 

                _cachedConnection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly;Cache=Shared;");
                await _cachedConnection.OpenAsync();

                var pragmaCmd = _cachedConnection.CreateCommand();
                pragmaCmd.CommandText = "PRAGMA synchronous = OFF; PRAGMA cache_size = 10;";
                await pragmaCmd.ExecuteNonQueryAsync();

                _cachedCommand = _cachedConnection.CreateCommand();
                _cachedCommand.CommandText = "SELECT version FROM CmdPalVisualStudioCodeHistoryRecentlyOpenedPathsListTracker";
            }
        }

        private void CloseConnection()
        {
            _cachedCommand?.Dispose();
            _cachedCommand = null;

            _cachedConnection?.Close();
            _cachedConnection?.Dispose();
            _cachedConnection = null;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            CloseConnection();
            GC.SuppressFinalize(this);
        }
    }
}