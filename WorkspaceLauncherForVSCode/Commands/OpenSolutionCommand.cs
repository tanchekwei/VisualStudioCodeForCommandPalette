// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Components;
using WorkspaceLauncherForVSCode.Helpers;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Commands;

public partial class OpenSolutionCommand : InvokableCommand, IHasWorkspace
{
    public VisualStudioCodeWorkspace Workspace { get; set; }
    private readonly VisualStudioCodePage? page;
    private readonly bool _elevated;

    public OpenSolutionCommand(VisualStudioCodeWorkspace workspace, VisualStudioCodePage page, bool elevated = false)
    {
        Workspace = workspace;
        this.page = page;
        _elevated = elevated;

        if (_elevated)
        {
            Name = "Run as Administrator";
            this.Icon = new("\uE7EF");
        }
        else
        {
            Name = "Open";
            this.Icon = Classes.Icon.VisualStudio;
        }
    }

    public override CommandResult Invoke()
    {
        if (Workspace.WindowsPath is not null)
        {
            var pathNotFoundResult = CommandHelpers.IsPathValid(Workspace.WindowsPath);
            if (pathNotFoundResult != null)
            {
                return pathNotFoundResult;
            }
        }

        if (string.IsNullOrEmpty(Workspace.Path))
        {
            return CommandResult.Dismiss();
        }

        OpenWindows.Instance.UpdateVisualStudioWindowsList();
        var visualStudioWindows = OpenWindows.Instance.Windows;
        var matchedElevatedVisualStudioWindows = new List<Window>();
        foreach (var window in visualStudioWindows)
        {
            if (window.IsProcessElevated)
            {
                //var solutionName = System.IO.Path.GetFileNameWithoutExtension(Workspace.Path);
                //if (window.Title.Contains(solutionName))
                //{
                //    matchedElevatedVisualStudioWindows.Add(window);
                //}
            }
            else
            {
                string? commandLine = NativeProcessCommandLine.GetCommandLine(window.Process.Process);
                string? solutionPath = NativeProcessCommandLine.ExtractSolutionPath(commandLine);
                if (solutionPath == Workspace.WindowsPath)
                {
                    window.SwitchToWindow();
                    return PageCommandResultHandler.HandleCommandResult(page);
                }
            }
        }

        //if (matchedElevatedVisualStudioWindows.Count > 0)
        //{
            // TODO: Waiting on https://github.com/microsoft/PowerToys/pull/38025 for GotoPage support.
            // Since we can't retrieve the command line from elevated processes, we can't determine their solution path.
            // If any elevated Visual Studio windows have a title that contains the solution name,
            // consider navigating to a selection page to let the user choose which window to switch to or reopen the solution.
        //}

        if (Workspace.VSInstance != null)
        {
            OpenInShellHelper.OpenInShell(Workspace.VSInstance.InstancePath, Workspace.Path, runAs: _elevated ? OpenInShellHelper.ShellRunAsType.Administrator : OpenInShellHelper.ShellRunAsType.None);
        }

        if (page != null)
        {
            Task.Run(() => page.UpdateFrequencyAsync(Workspace.Path));
        }

        return PageCommandResultHandler.HandleCommandResult(page);
    }
}
