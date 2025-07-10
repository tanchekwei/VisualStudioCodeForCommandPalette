using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace WorkspaceLauncherForVSCode.Helpers
{
    internal static class NativeProcessCommandLine
    {
        internal static string? GetCommandLine(Process process)
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
                byte[] buffer = new byte[commandLine.Length];
                if (!NativeMethods.ReadProcessMemory(hProcess, commandLine.Buffer, buffer, buffer.Length, out _))
                    return null;

                return Encoding.Unicode.GetString(buffer);
            }
            finally
            {
                NativeMethods.CloseHandle(hProcess);
            }
        }

        internal static string? ExtractSolutionPath(string? commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return null;

            var args = commandLine.Trim();

            // Remove the quoted executable path
            if (args.StartsWith('\"'))
            {
                var endQuote = args.IndexOf('"', 1);
                if (endQuote < 0 || endQuote + 1 >= args.Length)
                    return null;

                args = args[(endQuote + 1)..].Trim();
            }
            else
            {
                var firstSpace = args.IndexOf(' ');
                if (firstSpace < 0 || firstSpace + 1 >= args.Length)
                    return null;

                args = args[(firstSpace + 1)..].Trim();
            }

            // First argument is the solution path (possibly quoted)
            if (args.StartsWith('\"'))
            {
                var endQuote = args.IndexOf('"', 1);
                return endQuote > 0 ? args[1..endQuote] : null;
            }

            var firstSpaceUnquoted = args.IndexOf(' ');
            return firstSpaceUnquoted > 0 ? args[..firstSpaceUnquoted] : args;
        }
    }
}
