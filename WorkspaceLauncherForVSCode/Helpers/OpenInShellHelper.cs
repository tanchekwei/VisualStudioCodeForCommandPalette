// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace Microsoft.CmdPal.Ext.System.Helpers;

public static partial class OpenInShellHelper
{
    public static bool OpenInShell(string path, string? arguments = null, string? workingDir = null, ShellRunAsType runAs = ShellRunAsType.None, bool runWithHiddenWindow = false)
    {
        try
        {
            using var process = new Process();
            process.StartInfo.FileName = path;
            process.StartInfo.WorkingDirectory = string.IsNullOrWhiteSpace(workingDir) ? string.Empty : workingDir;
            process.StartInfo.Arguments = string.IsNullOrWhiteSpace(arguments) ? string.Empty : arguments;
            process.StartInfo.WindowStyle = runWithHiddenWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
            process.StartInfo.UseShellExecute = true;

            if (runAs == ShellRunAsType.Administrator)
            {
                process.StartInfo.Verb = "RunAs";
            }
            else if (runAs == ShellRunAsType.OtherUser)
            {
                process.StartInfo.Verb = "RunAsUser";
            }

            process.Start();
            return true;
        }
        catch (Win32Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage() { Message = $"Unable to open {path}: {ex.Message}" });
            ErrorLogger.LogError(ex);
            return false;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return false;
        }
    }

    public enum ShellRunAsType
    {
        None,
        Administrator,
        OtherUser,
    }
}
