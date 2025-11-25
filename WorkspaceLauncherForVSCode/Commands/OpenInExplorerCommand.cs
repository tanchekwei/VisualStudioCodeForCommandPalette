// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.IO;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Commands
{
    public sealed partial class OpenInExplorerCommand : InvokableCommand
    {
        private readonly VisualStudioCodeWorkspace? workspace;
        private readonly string _path;
        private string _arguments;

        public OpenInExplorerCommand(string arguments, VisualStudioCodeWorkspace? workspace, string name = "Open in Explorer", string path = "explorer.exe")
        {
            try
            {
                Name = name;
                _path = path;
                _arguments = arguments;
                Icon = Classes.Icon.FileExplorer;
                this.workspace = workspace;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                throw;
            }
        }

        public override CommandResult Invoke()
        {
            try
            {
                string pathToOpen = workspace?.WindowsPath ?? _arguments;

                if (string.IsNullOrEmpty(pathToOpen))
                {
                    return CommandResult.Dismiss();
                }
                if (workspace?.WorkspaceType == WorkspaceType.Solution ||
                    workspace?.WorkspaceType == WorkspaceType.Solution2026 ||
                    workspace?.WorkspaceType == WorkspaceType.Workspace)
                {
                    pathToOpen = Path.GetDirectoryName(pathToOpen) ?? string.Empty;
                }

                if (string.IsNullOrEmpty(pathToOpen))
                {
                    new ToastStatusMessage($"Path does not exist").Show();
                    return CommandResult.KeepOpen();
                }

                var pathInvalidResult = CommandHelpers.IsPathValid(pathToOpen);
                if (pathInvalidResult != null)
                {
                    return pathInvalidResult;
                }

                OpenInShellHelper.OpenInShell(_path, pathToOpen);
                return CommandResult.Dismiss();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return CommandResult.KeepOpen();
            }
        }
    }
}
