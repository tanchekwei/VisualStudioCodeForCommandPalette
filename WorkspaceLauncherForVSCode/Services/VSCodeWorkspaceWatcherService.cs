// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Workspaces.Readers;

namespace WorkspaceLauncherForVSCode.Services
{
    public partial class VSCodeWorkspaceWatcherService : IVSCodeWorkspaceWatcherService, IDisposable
    {
        private Timer? _timer;
        private readonly IVisualStudioCodeService _vscodeService;
        private readonly SettingsManager _settingsManager;
        private List<VisualStudioCodeWorkspace> _lastKnownWorkspaces = new();
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
                if (_timer == null)
                {
                    _timer = new Timer(async _ => await CheckForChanges(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
                }
                else
                {
                    _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
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
            var currentWorkspaces = new List<VisualStudioCodeWorkspace>();
            var instances = _vscodeService.GetInstances();

            foreach (var instance in instances)
            {
                var workspaces = await VscdbWorkspaceReader.GetWorkspacesAsync(instance, CancellationToken.None);
                currentWorkspaces.AddRange(workspaces);
            }

            if (!_lastKnownWorkspaces.SequenceEqual(currentWorkspaces))
            {
                _lastKnownWorkspaces = currentWorkspaces;
                TriggerRefresh?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}