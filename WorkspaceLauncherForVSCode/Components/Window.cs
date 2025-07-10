// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Globalization;
using System.Text;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Components
{
    internal sealed class Window
    {
        private readonly IntPtr hwnd;
        private readonly WindowProcess processInfo;

        internal string Title
        {
            get
            {
                var sizeOfTitle = NativeMethods.GetWindowTextLength(hwnd);
                if (sizeOfTitle++ > 0)
                {
                    StringBuilder titleBuffer = new StringBuilder(sizeOfTitle);
                    var numCharactersWritten = NativeMethods.GetWindowText(hwnd, titleBuffer, sizeOfTitle);
                    if (numCharactersWritten == 0)
                    {
                        return string.Empty;
                    }
                    return titleBuffer.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        internal IntPtr Hwnd => hwnd;
        internal WindowProcess Process => processInfo;
        internal bool IsProcessElevated { get; set; }
        internal bool Visible => NativeMethods.IsWindowVisible(Hwnd);
        internal bool IsCloaked => GetWindowCloakState() != WindowCloakState.None;
        internal bool IsWindow => NativeMethods.IsWindow(Hwnd);
        internal bool IsToolWindow => (NativeMethods.GetWindowLong(Hwnd, Win32Constants.GWL_EXSTYLE) & (uint)ExtendedWindowStyles.WS_EX_TOOLWINDOW) == (uint)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
        internal bool IsAppWindow => (NativeMethods.GetWindowLong(Hwnd, Win32Constants.GWL_EXSTYLE) & (uint)ExtendedWindowStyles.WS_EX_APPWINDOW) == (uint)ExtendedWindowStyles.WS_EX_APPWINDOW;
        internal bool TaskListDeleted => NativeMethods.GetProp(Hwnd, "ITaskList_Deleted") != IntPtr.Zero;
        internal bool IsOwner => NativeMethods.GetWindow(Hwnd, GetWindowCmd.GW_OWNER) == IntPtr.Zero;
        internal bool Minimized => GetWindowSizeState() == WindowSizeState.Minimized;
        internal string ClassName => GetWindowClassName(Hwnd);

        internal Window(IntPtr hwnd)
        {
            this.hwnd = hwnd;
            processInfo = new WindowProcess(hwnd);
        }

        internal void SwitchToWindow()
        {
            if (!Minimized)
            {
                NativeMethods.SetForegroundWindow(Hwnd);
            }
            else
            {
                if (!NativeMethods.ShowWindow(Hwnd, ShowWindowCommand.Restore))
                {
                    _ = NativeMethods.SendMessage(Hwnd, Win32Constants.WM_SYSCOMMAND, Win32Constants.SC_RESTORE);
                }
            }
            NativeMethods.FlashWindow(Hwnd, true);
        }

        public override string ToString()
        {
            return Title + " (" + processInfo.Process.ProcessName?.ToUpper(CultureInfo.CurrentCulture) + ")";
        }

        internal WindowSizeState GetWindowSizeState()
        {
            NativeMethods.GetWindowPlacement(Hwnd, out WINDOWPLACEMENT placement);
            return placement.ShowCmd switch
            {
                ShowWindowCommand.Normal => WindowSizeState.Normal,
                ShowWindowCommand.Minimize or ShowWindowCommand.ShowMinimized => WindowSizeState.Minimized,
                ShowWindowCommand.Maximize => WindowSizeState.Maximized,
                _ => WindowSizeState.Unknown,
            };
        }

        internal enum WindowSizeState
        {
            Normal,
            Minimized,
            Maximized,
            Unknown,
        }

        internal WindowCloakState GetWindowCloakState()
        {
            _ = NativeMethods.DwmGetWindowAttribute(Hwnd, (int)DwmWindowAttributes.Cloaked, out var isCloakedState, sizeof(uint));
            return isCloakedState switch
            {
                (int)DwmWindowCloakStates.None => WindowCloakState.None,
                (int)DwmWindowCloakStates.CloakedApp => WindowCloakState.App,
                (int)DwmWindowCloakStates.CloakedShell => WindowCloakState.Shell,
                (int)DwmWindowCloakStates.CloakedInherited => WindowCloakState.Inherited,
                _ => WindowCloakState.Unknown,
            };
        }

        internal enum WindowCloakState
        {
            None,
            App,
            Shell,
            Inherited,
            Unknown,
        }

        private static string GetWindowClassName(IntPtr hwnd)
        {
            StringBuilder windowClassName = new StringBuilder(300);
            var numCharactersWritten = NativeMethods.GetClassName(hwnd, windowClassName, windowClassName.MaxCapacity);
            return numCharactersWritten == 0 ? string.Empty : windowClassName.ToString();
        }
    }
}