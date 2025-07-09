// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.IO;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
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
            Name = name;
            _path = path;
            _arguments = arguments;
            Icon = Classes.Icon.FileExplorer;
            this.workspace = workspace;
        }

        public override CommandResult Invoke()
        {
            string pathToOpen = workspace?.WindowsPath ?? _arguments;

            if (string.IsNullOrEmpty(pathToOpen))
            {
                return CommandResult.Dismiss();
            }
            if (workspace?.WorkspaceType == Enums.WorkspaceType.Solution || workspace?.WorkspaceType == WorkspaceType.Workspace)
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
    }
}
