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
    public partial class WorkspaceWatcherService : IWorkspaceWatcherService, IDisposable
    {
        private readonly Timer _timer;
        private readonly IVisualStudioCodeService _vscodeService;
        private readonly SettingsManager _settingsManager;
        private List<VisualStudioCodeWorkspace> _lastKnownWorkspaces = new();

        public event EventHandler? TriggerRefresh;

        public WorkspaceWatcherService( IVisualStudioCodeService vscodeService, SettingsManager settingsManager)
        {
            _vscodeService = vscodeService;
            _settingsManager = settingsManager;
            _timer = new Timer(async _ => await CheckForChanges(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public void StartWatching()
        {
            if (_settingsManager.EnableWorkspaceWatcher && _settingsManager.SortBy == Enums.SortBy.RecentFromVSCode)
            {
                _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }
        }

        public void StopWatching()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
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