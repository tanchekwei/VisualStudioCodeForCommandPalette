// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.IO;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Commands
{
    public partial class OpenInTerminalCommand : InvokableCommand, IHasWorkspace
    {
        public VisualStudioCodeWorkspace Workspace { get; set; }
        private readonly SettingsManager _settingsManager;

        public OpenInTerminalCommand(VisualStudioCodeWorkspace workspace, SettingsManager settingsManager)
        {
            Workspace = workspace;
            _settingsManager = settingsManager;
            Name = "Open in Terminal";
            Icon = new("\uE756");
        }

        public override CommandResult Invoke()
        {
            if (Workspace.WindowsPath is not null)
            {
                string directoryPath;
                if (File.Exists(Workspace.WindowsPath))
                {
                    directoryPath = Path.GetDirectoryName(Workspace.WindowsPath) ?? "";
                }
                else
                {
                    directoryPath = Workspace.WindowsPath;
                }

                if (string.IsNullOrEmpty(directoryPath))
                {
                    return CommandResult.Dismiss();
                }

                var pathNotFoundResult = CommandHelpers.IsPathValid(directoryPath);
                if (pathNotFoundResult != null)
                {
                    return pathNotFoundResult;
                }

                var terminal = _settingsManager.TerminalType == TerminalType.PowerShell ? "powershell.exe" : "cmd.exe";
                OpenInShellHelper.OpenInShell(terminal, workingDir: directoryPath);
            }

            return CommandResult.Dismiss();
        }
    }
}