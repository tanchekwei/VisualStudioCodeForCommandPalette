// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Diagnostics;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Components
{
    internal sealed class WindowProcess
    {
        private readonly uint processId;
        private Process? process;
        private bool processResolved;

        internal uint ProcessId => processId;

        internal Process? Process
        {
            get
            {
                if (!processResolved)
                {
                    processResolved = true;
                    if (processId != 0)
                    {
                        try
                        {
                            process = Process.GetProcessById((int)processId);
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError(ex);
                        }
                    }
                }
                return process;
            }
        }

        internal WindowProcess(IntPtr hwnd)
        {
            try
            {
                _ = NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
                processId = pid;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }
    }
}