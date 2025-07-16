// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;

namespace WorkspaceLauncherForVSCode.Interfaces
{
    public interface IWorkspaceWatcherService
    {
        event EventHandler? TriggerRefresh;
        void StartWatching();
        void StopWatching();
    }
}