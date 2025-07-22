// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Commands
{
    internal static class CommandHelpers
    {
        public static CommandResult? IsPathValid(string path)
        {
            try
            {
                if (!Directory.Exists(path) && !File.Exists(path))
                {
                    new ToastStatusMessage($"Path does not exist: {path}").Show();
                    return CommandResult.KeepOpen();
                }
                return null;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return CommandResult.KeepOpen();
            }
        }
    }
}