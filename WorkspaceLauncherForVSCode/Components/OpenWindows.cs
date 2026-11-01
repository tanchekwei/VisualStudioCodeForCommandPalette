// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Components
{
    internal sealed class OpenWindows
    {
        private static readonly object _enumWindowsLock = new();
        private readonly List<Window> windows = new();
        private readonly EnumWindowsProc _enumWindowsCallback;
        private static OpenWindows? instance;

        internal IReadOnlyList<Window> Windows => windows;

        internal static OpenWindows Instance
        {
            get
            {
                instance ??= new OpenWindows();
                return instance;
            }
        }

        private OpenWindows()
        {
            _enumWindowsCallback = VisualStudioWindowEnumerationCallBack;
        }

        internal void UpdateVisualStudioWindowsList()
        {
            try
            {
                lock (_enumWindowsLock)
                {
                    windows.Clear();
                    _ = NativeMethods.EnumWindows(_enumWindowsCallback, IntPtr.Zero);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        private bool VisualStudioWindowEnumerationCallBack(IntPtr hwnd, IntPtr lParam)
        {
            try
            {
                // Fast pre-filter using lightweight Win32 APIs before creating expensive Window/Process objects
                if (!NativeMethods.IsWindow(hwnd) || !NativeMethods.IsWindowVisible(hwnd))
                {
                    return true;
                }

                if (NativeMethods.GetWindow(hwnd, GetWindowCmd.GW_OWNER) != IntPtr.Zero)
                {
                    return true;
                }

                var exStyle = NativeMethods.GetWindowLong(hwnd, Win32Constants.GWL_EXSTYLE);
                bool isToolWindow = (exStyle & (uint)ExtendedWindowStyles.WS_EX_TOOLWINDOW) == (uint)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                bool isAppWindow = (exStyle & (uint)ExtendedWindowStyles.WS_EX_APPWINDOW) == (uint)ExtendedWindowStyles.WS_EX_APPWINDOW;
                if (isToolWindow && !isAppWindow)
                {
                    return true;
                }

                if (NativeMethods.GetProp(hwnd, "ITaskList_Deleted") != IntPtr.Zero)
                {
                    return true;
                }

                Window newWindow = new Window(hwnd);

                if (newWindow.ClassName != "Windows.UI.Core.CoreWindow" && !newWindow.IsCloaked)
                {
                    if (newWindow.Process?.Process != null && string.Equals(newWindow.Process.Process.ProcessName, "devenv", StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsProcessElevated((int)newWindow.Process.ProcessId))
                        {
                            newWindow.IsProcessElevated = true;
                        }
                        windows.Add(newWindow);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }

            return true;
        }

        private static bool IsProcessElevated(int processId)
        {
            try
            {
                IntPtr hProcess = NativeMethods.OpenProcess(0x1000 /* PROCESS_QUERY_INFORMATION */, false, processId);
                if (hProcess == IntPtr.Zero)
                    return false;

                IntPtr hToken = IntPtr.Zero;
                try
                {
                    if (!NativeMethods.OpenProcessToken(hProcess, 0x0008 /* TOKEN_QUERY */, out hToken))
                        return false;

                    if (NativeMethods.GetTokenInformation(hToken, NativeMethods.TokenElevation, out int elevation, sizeof(int), out _))
                    {
                        return elevation != 0;
                    }
                }
                finally
                {
                    if (hToken != IntPtr.Zero)
                        NativeMethods.CloseHandle(hToken);
                    NativeMethods.CloseHandle(hProcess);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }

            return false;
        }
    }
}