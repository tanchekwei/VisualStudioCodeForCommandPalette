// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Classes
{
    internal static class ErrorLogger
    {
        private static readonly string LogDir = Utilities.BaseSettingsPath(Constant.AppName);
        private static readonly string LogFilePath = Path.Combine(LogDir, "error.log");
        private static readonly object _lock = new object();

        private const long MaxLogSizeBytes = 1 * 1024 * 1024; // 1 MB
        private const int MaxArchivedLogs = 5;

        public static void LogError(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            try
            {
                lock (_lock)
                {
                    Directory.CreateDirectory(LogDir);

                    if (File.Exists(LogFilePath) && new FileInfo(LogFilePath).Length >= MaxLogSizeBytes)
                    {
                        RollLogFiles();
                    }

                    var className = Path.GetFileNameWithoutExtension(sourceFilePath);
                    var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{className}.{memberName}] {ex}{Environment.NewLine}";
                    File.AppendAllText(LogFilePath, logMessage);
                }
            }
            catch
            {
                // Suppress exceptions during logging to avoid crashing the app.
            }
        }

        private static void RollLogFiles()
        {
            try
            {
                var oldest = Path.Combine(LogDir, $"error_{MaxArchivedLogs}.log");
                if (File.Exists(oldest))
                {
                    File.Delete(oldest);
                }

                for (int i = MaxArchivedLogs - 1; i >= 1; i--)
                {
                    var src = Path.Combine(LogDir, $"error_{i}.log");
                    var dest = Path.Combine(LogDir, $"error_{i + 1}.log");

                    if (File.Exists(src))
                    {
                        File.Move(src, dest, overwrite: true);
                    }
                }

                if (File.Exists(LogFilePath))
                {
                    File.Move(LogFilePath, Path.Combine(LogDir, "error_1.log"), overwrite: true);
                }
            }
            catch
            {
                // Don't let log rotation failure affect the app
            }
        }
    }
}
