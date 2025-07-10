// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Diagnostics;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Components
{
    internal sealed class WindowProcess
    {
        private readonly uint processId;
        private readonly Process? process;
        internal uint ProcessId => processId;
        internal Process? Process => process;
        internal WindowProcess(IntPtr hwnd)
        {
            _ = NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
            processId = pid;
            process = Process.GetProcessById((int)processId);
        }
    }
}
