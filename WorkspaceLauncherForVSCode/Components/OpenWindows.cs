// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Components
{
    internal sealed class OpenWindows
    {
        private static readonly object _enumWindowsLock = new();
        private readonly List<Window> windows = new();
        private static OpenWindows? instance;

        internal List<Window> Windows => new(windows);

        internal static OpenWindows Instance
        {
            get
            {
                instance ??= new OpenWindows();
                return instance;
            }
        }

        private OpenWindows() { }

        internal void UpdateVisualStudioWindowsList()
        {
            lock (_enumWindowsLock)
            {
                windows.Clear();
                EnumWindowsProc callbackptr = new EnumWindowsProc(VisualStudioWindowEnumerationCallBack);
                _ = NativeMethods.EnumWindows(callbackptr, IntPtr.Zero);
            }
        }

        private bool VisualStudioWindowEnumerationCallBack(IntPtr hwnd, IntPtr lParam)
        {
            Window newWindow = new Window(hwnd);

            if (newWindow.IsWindow && newWindow.Visible && newWindow.IsOwner &&
                (!newWindow.IsToolWindow || newWindow.IsAppWindow) && !newWindow.TaskListDeleted &&
                newWindow.ClassName != "Windows.UI.Core.CoreWindow" &&
                !newWindow.IsCloaked)
            {
                if (newWindow.Process?.Process != null && string.Equals(newWindow.Process.Process.ProcessName, "devenv", StringComparison.OrdinalIgnoreCase))
                {
                    if (newWindow.Process.Process is Process proc && IsProcessElevated(proc.Id))
                    {
                        newWindow.IsProcessElevated = true;
                    }
                    windows.Add(newWindow);
                }
            }

            return true;
        }

        private static bool IsProcessElevated(int processId)
        {
            IntPtr hProcess = NativeMethods.OpenProcess(0x1000 /* PROCESS_QUERY_INFORMATION */, false, processId);
            if (hProcess == IntPtr.Zero)
                return false;

            IntPtr hToken = IntPtr.Zero;
            try
            {
                if (!NativeMethods.OpenProcessToken(hProcess, 0x0008 /* TOKEN_QUERY */, out hToken))
                    return false;

                uint tokenInfoLength = (uint)Marshal.SizeOf<int>();
                IntPtr elevationPtr = Marshal.AllocHGlobal((int)tokenInfoLength);
                try
                {
                    if (NativeMethods.GetTokenInformation(hToken, NativeMethods.TokenElevation, elevationPtr, tokenInfoLength, out _))
                    {
                        int elevation = Marshal.ReadInt32(elevationPtr);
                        return elevation != 0;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(elevationPtr);
                }
            }
            finally
            {
                if (hToken != IntPtr.Zero)
                    NativeMethods.CloseHandle(hToken);
                NativeMethods.CloseHandle(hProcess);
            }

            return false;
        }
    }
}
