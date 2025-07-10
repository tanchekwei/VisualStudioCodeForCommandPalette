// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable SA1649, CA1051, CA1707, CA1028, CA1714, CA1069, SA1402

namespace WorkspaceLauncherForVSCode.Helpers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "These are the names used by win32.")]
    internal static partial class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int EnumWindows(EnumWindowsProc callPtr, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1838:Avoid 'StringBuilder' parameters for P/Invokes", Justification = "Minimal change")]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommand nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("psapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1838:Avoid 'StringBuilder' parameters for P/Invokes", Justification = "Minimal change")]
        public static extern uint GetProcessImageFileName(IntPtr hProcess, [Out] StringBuilder lpImageFileName, [In][MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("user32.dll", SetLastError = true, BestFitMapping = false, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetProp(IntPtr hWnd, string lpString);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

        [DllImport("user32.dll", BestFitMapping = false, CharSet = CharSet.Unicode)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1838:Avoid 'StringBuilder' parameters for P/Invokes", Justification = "Minimal change")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        public const int TokenElevation = 20;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr tokenHandle, int tokenInfoClass,
            IntPtr tokenInfo, uint tokenInfoLength, out uint returnLength);

        [DllImport("ntdll.dll")]
        public static extern int NtQueryInformationProcess(
            IntPtr processHandle,
            int processInformationClass,
            out PROCESS_BASIC_INFORMATION processInformation,
            int processInformationLength,
            out int returnLength);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] out IntPtr lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] out UNICODE_STRING lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);
    }

    internal static class Win32Constants
    {
        public const int GWL_EXSTYLE = -20;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_RESTORE = 0xf120;
    }

    internal delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    internal enum ShowWindowCommand
    {
        Hide = 0,
        Normal = 1,
        ShowMinimized = 2,
        Maximize = 3,
        ShowMaximized = 3,
        ShowNoActivate = 4,
        Show = 5,
        Minimize = 6,
        ShowMinNoActive = 7,
        ShowNA = 8,
        Restore = 9,
        ShowDefault = 10,
        ForceMinimize = 11,
    }

    [Flags]
    internal enum DwmWindowAttributes
    {
        Cloaked = 14,
    }

    [Flags]
    internal enum DwmWindowCloakStates
    {
        None = 0,
        CloakedApp = 1,
        CloakedShell = 2,
        CloakedInherited = 4,
    }

    [Flags]
    internal enum ProcessAccess
    {
        VmRead = 0x0010,
        QueryLimitedInformation = 0x1000,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPLACEMENT : IEquatable<WINDOWPLACEMENT>
    {
        public int Length;
        public int Flags;
        public ShowWindowCommand ShowCmd;
        public POINT MinPosition;
        public POINT MaxPosition;
        public RECT NormalPosition;

        public static WINDOWPLACEMENT Default
        {
            get
            {
                WINDOWPLACEMENT result = default;
                result.Length = Marshal.SizeOf(result);
                return result;
            }
        }

        public static bool operator ==(WINDOWPLACEMENT left, WINDOWPLACEMENT right)
        {
            return left.Length == right.Length
                && left.Flags == right.Flags
                && left.ShowCmd == right.ShowCmd
                && left.MinPosition == right.MinPosition
                && left.MaxPosition == right.MaxPosition
                && left.NormalPosition == right.NormalPosition;
        }

        public static bool operator !=(WINDOWPLACEMENT left, WINDOWPLACEMENT right)
        {
            return !(left == right);
        }

        public bool Equals(WINDOWPLACEMENT other)
        {
            return this == other;
        }

        public override bool Equals(object? obj)
        {
            if (obj is WINDOWPLACEMENT wp)
            {
                return this == wp;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Length, Flags, ShowCmd, MinPosition, MaxPosition, NormalPosition);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT : IEquatable<RECT>
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public static bool operator ==(RECT r1, RECT r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(RECT r1, RECT r2)
        {
            return !r1.Equals(r2);
        }

        public bool Equals(RECT other)
        {
            return other.Left == Left && other.Top == Top && other.Right == Right && other.Bottom == Bottom;
        }

        public override bool Equals(object? obj)
        {
            if (obj is RECT rect)
            {
                return Equals(rect);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT : IEquatable<POINT>
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj)
        {
            if (obj is POINT pt)
            {
                return this.X == pt.X && this.Y == pt.X;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(POINT left, POINT right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(POINT left, POINT right)
        {
            return !(left == right);
        }

        public bool Equals(POINT other)
        {
            return this == other;
        }
    }

    internal enum GetWindowCmd : uint
    {
        GW_OWNER = 4,
    }

    [Flags]
    internal enum ExtendedWindowStyles : uint
    {
        WS_EX_TOOLWINDOW = 0x0080,
        WS_EX_APPWINDOW = 0x40000,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_BASIC_INFORMATION
    {
        public IntPtr Reserved1;
        public IntPtr PebBaseAddress;
        public IntPtr Reserved2_0;
        public IntPtr Reserved2_1;
        public IntPtr UniqueProcessId;
        public IntPtr Reserved3;
    }

    [Flags]
    enum ProcessAccessFlags : uint
    {
        QueryInformation = 0x0400,
        VirtualMemoryRead = 0x0010
    }
}