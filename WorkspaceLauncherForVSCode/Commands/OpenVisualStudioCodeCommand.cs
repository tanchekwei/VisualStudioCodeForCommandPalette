// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Commands;

/// <summary>
/// Command to open a Visual Studio Code workspace.
/// </summary>
internal sealed partial class OpenVisualStudioCodeCommand : InvokableCommand, IHasWorkspace
{
    private readonly VisualStudioCodePage page;
    private readonly bool _elevated;

    public VisualStudioCodeWorkspace Workspace { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenVisualStudioCodeCommand"/> class.
    /// </summary>
    /// <param name="workspace">The Visual Studio Code workspace to open.</param>
    /// <param name="page">The Visual Studio Code page instance.</param>
    public OpenVisualStudioCodeCommand(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page, bool elevated = false)
    {
        try
        {
            Workspace = workspace;
            this.page = page;
            _elevated = elevated;
            this.Icon = Classes.Icon.VisualStudioCode;

            if (elevated)
            {
                Name = "Run as Administrator";
                this.Icon = new("\uE7EF");
            }
            else
            {
                Name = $"Open";
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    /// <summary>
    /// Invokes the command to open the workspace in Visual Studio Code.
    /// </summary>
    /// <returns>The result of the command execution.</returns>
    public override CommandResult Invoke()
    {
        try
        {
            if (Workspace.WorkspaceType == WorkspaceType.Solution)
            {
                return CommandResult.Confirm(new ConfirmationArgs { Title = "Error", Description = "Cannot open a solution with this command." });
            }

            if (Workspace.Path is null || Workspace.VSCodeInstance is null)
            {
                return CommandResult.Confirm(new ConfirmationArgs { Title = "Error", Description = "Workspace path, or instance is null. Cannot open." });
            }

            var pathToValidate = Workspace.WindowsPath ?? Workspace.Path;
            if (Workspace.VisualStudioCodeRemoteUri == null)
            {
                var pathInvalidResult = CommandHelpers.IsPathValid(pathToValidate);
                if (pathInvalidResult != null)
                {
                    return pathInvalidResult;
                }
            }

            string? arguments;
            // Open the workspace in Visual Studio Code
            if (Workspace.Path.EndsWith(".code-workspace", System.StringComparison.OrdinalIgnoreCase))
            {
                arguments = $"--file-uri \"{Workspace.Path}\"";
            }
            else
            {
                arguments = $"--folder-uri \"{Workspace.Path}\"";
            }

            OpenInShellHelper.OpenInShell(Workspace.VSCodeInstance.ExecutablePath, arguments, runAs: _elevated ? OpenInShellHelper.ShellRunAsType.Administrator : OpenInShellHelper.ShellRunAsType.None);

            Task.Run(() => page.UpdateFrequencyAsync(Workspace.Path));

            return PageCommandResultHandler.HandleCommandResult(page);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return CommandResult.KeepOpen();
        }
    }
}
