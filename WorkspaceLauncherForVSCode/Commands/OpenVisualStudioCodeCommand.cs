// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
    private readonly bool _isFromVisualStudioSolution;

    public VisualStudioCodeWorkspace Workspace { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenVisualStudioCodeCommand"/> class.
    /// </summary>
    /// <param name="workspace">The Visual Studio Code workspace to open.</param>
    /// <param name="page">The Visual Studio Code page instance.</param>
    /// <param name="elevated">Whether to run as administrator.</param>
    /// <param name="isFromVisualStudioSolution">Whether this command is opening a Visual Studio solution.</param>
    public OpenVisualStudioCodeCommand(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page, bool elevated = false, bool isFromVisualStudioSolution = false)
    {
        try
        {
            Workspace = workspace;
            this.page = page;
            _elevated = elevated;
            _isFromVisualStudioSolution = isFromVisualStudioSolution;
            this.Icon = Classes.Icon.VisualStudioCode;

            if (elevated)
            {
                Name = "Run as Administrator";
                this.Icon = new("\uE7EF");
            }
            else
            {
                if (isFromVisualStudioSolution)
                {
                    Name = "Open in Visual Studio Code";
                }
                else
                {
                    Name = "Open";
                }
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
            if (!_isFromVisualStudioSolution && (Workspace.WorkspaceType == WorkspaceType.Solution || Workspace.WorkspaceType == WorkspaceType.Solution2026))
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
            if (Workspace.Path.EndsWith(".code-workspace", StringComparison.OrdinalIgnoreCase))
            {
                arguments = $"--file-uri \"{Workspace.Path}\"";
            }
            else
            {
                var path = Workspace.Path;
                if (_isFromVisualStudioSolution)
                {
                    if (File.Exists(path) && !Directory.Exists(path))
                    {
                        path = Path.GetDirectoryName(path);
                    }

                    if (path != null)
                    {
                        var uri = new Uri(path);
                        path = uri.AbsoluteUri;
                    }
                }

                arguments = $"--folder-uri \"{path}\"";
            }

            if (page.SettingsManager.UseHelperLauncher)
            {
                var launcherPath = Path.Combine(AppContext.BaseDirectory, "VisualStudioCodeForCommandPaletteLauncher.exe");
                var launcherArgs = $"\"{Workspace.VSCodeInstance.ExecutablePath}\" {arguments}";

                var startInfo = new ProcessStartInfo(launcherPath, launcherArgs)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };

                if (_elevated)
                {
                    startInfo.Verb = "runas";
                    startInfo.UseShellExecute = true;
                }

                Process.Start(startInfo);
            }
            else
            {
                ShellHelpers.OpenInShell(Workspace.VSCodeInstance.ExecutablePath, arguments, runAs: _elevated ? ShellHelpers.ShellRunAsType.Administrator : ShellHelpers.ShellRunAsType.None);
            }

            Task.Run(() => page.UpdateFrequencyAsync(Workspace.Path, Workspace.WorkspaceType));

            return PageCommandResultHandler.HandleCommandResult(page);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return CommandResult.KeepOpen();
        }
    }
}
