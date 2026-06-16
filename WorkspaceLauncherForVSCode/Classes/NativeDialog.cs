// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Runtime.InteropServices;
namespace WorkspaceLauncherForVSCode.Classes;

public static class NativeDialog
{
    public static void Show(string title, string message)
    {
        _ = MessageBoxW(IntPtr.Zero, message, title, 0);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(
        IntPtr hWnd,
        string text,
        string caption,
        uint type);
}
