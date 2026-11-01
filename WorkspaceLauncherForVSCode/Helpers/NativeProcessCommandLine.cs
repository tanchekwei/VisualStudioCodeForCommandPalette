using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Helpers
{
    internal static class NativeProcessCommandLine
    {
        internal static string? GetCommandLine(Process process)
        {
            try
            {
                IntPtr hProcess = NativeMethods.OpenProcess((int)(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead), false, process.Id);
                if (hProcess == IntPtr.Zero)
                    return null;

                try
                {
                    // NtQueryInformationProcess
                    int status = NativeMethods.NtQueryInformationProcess(hProcess, 0, out PROCESS_BASIC_INFORMATION pbi, Marshal.SizeOf<PROCESS_BASIC_INFORMATION>(), out _);
                    if (status != 0)
                        return null;

                    // Read PEB
                    IntPtr pebAddress = pbi.PebBaseAddress;
                    IntPtr rtlUserProcParamsAddress;

                    // Read address of ProcessParameters
                    if (!NativeMethods.ReadProcessMemory(hProcess, pebAddress + 0x20 /* offset of ProcessParameters */, out rtlUserProcParamsAddress, IntPtr.Size, out _))
                        return null;

                    // Read CommandLine UNICODE_STRING structure
                    UNICODE_STRING commandLine;
                    if (!NativeMethods.ReadProcessMemory(hProcess, rtlUserProcParamsAddress + 0x70 /* offset of CommandLine */, out commandLine, Marshal.SizeOf<UNICODE_STRING>(), out _))
                        return null;

                    // Read actual command line string
                    if (commandLine.Length == 0 || commandLine.Buffer == IntPtr.Zero)
                        return string.Empty;

                    IntPtr buffer = Marshal.AllocHGlobal(commandLine.Length);
                    try
                    {
                        if (!NativeMethods.ReadProcessMemory(hProcess, commandLine.Buffer, buffer, commandLine.Length, out _))
                            return null;

                        return Marshal.PtrToStringUni(buffer, commandLine.Length / sizeof(char));
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
                finally
                {
                    NativeMethods.CloseHandle(hProcess);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return null;
            }
        }

        internal static string? ExtractSolutionPath(string? commandLine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commandLine))
                    return null;

                var args = commandLine.AsSpan().Trim();

                // Remove the quoted executable path
                if (args.Length > 0 && args[0] == '"')
                {
                    var endQuote = args[1..].IndexOf('"');
                    if (endQuote < 0)
                        return null;

                    var afterQuoteIndex = endQuote + 2;
                    if (afterQuoteIndex >= args.Length)
                        return null;

                    args = args[afterQuoteIndex..].TrimStart();
                }
                else
                {
                    var firstSpace = args.IndexOf(' ');
                    if (firstSpace < 0)
                        return null;

                    args = args[(firstSpace + 1)..].TrimStart();
                }

                // First argument is the solution path (possibly quoted)
                if (args.Length > 0 && args[0] == '"')
                {
                    var endQuote = args[1..].IndexOf('"');
                    return endQuote >= 0 ? args.Slice(1, endQuote).ToString() : null;
                }

                var firstSpaceUnquoted = args.IndexOf(' ');
                return firstSpaceUnquoted > 0 ? args[..firstSpaceUnquoted].ToString() : args.ToString();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return null;
            }
        }
    }
}