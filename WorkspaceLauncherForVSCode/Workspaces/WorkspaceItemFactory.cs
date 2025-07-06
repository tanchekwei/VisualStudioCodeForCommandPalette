// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Workspaces
{
    public static class WorkspaceItemFactory
    {
        public static readonly Tag PinTag = new Tag();
        static WorkspaceItemFactory()
        {
            PinTag.Icon = Icon.Pinned;
        }

        public static ListItem Create(
            VisualStudioCodeWorkspace workspace,
            IVisualStudioCodePage page,
            IWorkspaceStorage workspaceStorage,
            SettingsManager settingsManager,
            CommandContextItem refreshCommandContextItem,
            CommandContextItem helpCommandContextItem)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            ICommand command;
            IconInfo icon;
            Details details;
            var tags = new List<Tag>();
            List<CommandContextItem> moreCommands = new();

            switch (workspace.WorkspaceType)
            {
                case WorkspaceType.Solution:
                    command = new OpenSolutionCommand(workspace, page, settingsManager.CommandResult);
                    icon = Classes.Icon.VisualStudio;
                    workspace.WindowsPath = workspace.Path;
                    details = new Details
                    {
                        Title = workspace.Name ?? string.Empty,
                        HeroImage = icon,
                        Metadata = workspace.Details,
                    };
                    if (settingsManager.TagTypes.HasFlag(TagType.Target))
                    {
                        if (workspace.VSInstance?.Name is string name)
                        {
                            tags.Add(new Tag(name));
                        }
                    }
                    if (settingsManager.VSSecondaryCommand == SecondaryCommand.OpenAsAdministrator)
                    {
                        moreCommands.Add(new CommandContextItem(new OpenSolutionCommand(workspace, page, settingsManager.CommandResult, elevated: true)));
                        if (!string.IsNullOrEmpty(workspace.WindowsPath))
                        {
                            moreCommands.Add(new CommandContextItem(new OpenInExplorerCommand(workspace.WindowsPath, workspace)));
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(workspace.WindowsPath))
                        {
                            moreCommands.Add(new CommandContextItem(new OpenInExplorerCommand(workspace.WindowsPath, workspace)));
                        }
                        moreCommands.Add(new CommandContextItem(new OpenSolutionCommand(workspace, page, settingsManager.CommandResult, elevated: true)));
                    }
                    break;
                default:
                    command = new OpenVisualStudioCodeCommand(workspace, page, settingsManager.CommandResult);
                    icon = Classes.Icon.VisualStudioCode;
                    details = new Details
                    {
                        Title = workspace.WorkspaceName,
                        HeroImage = icon,
                        Metadata = workspace.Details,
                    };
                    if (settingsManager.TagTypes.HasFlag(TagType.Type))
                    {
                        tags.Add(new Tag(workspace.WorkspaceTypeString));
                        if (workspace.VsCodeRemoteType != VsCodeRemoteType.Local)
                        {
                            tags.Add(new Tag(workspace.VsCodeRemoteType.ToString()));
                        }
                    }
                    if (settingsManager.TagTypes.HasFlag(TagType.Target))
                    {
                        if (workspace.VSCodeInstance?.Name is string name)
                        {
                            tags.Add(new Tag(name));
                        }
                    }

                    if (workspace.VsCodeRemoteType != VsCodeRemoteType.Remote)
                    {
                        if (settingsManager.VSCodeSecondaryCommand == SecondaryCommand.OpenAsAdministrator)
                        {
                            moreCommands.Add(new CommandContextItem(new OpenVisualStudioCodeCommand(workspace, page, settingsManager.CommandResult, elevated: true)));
                            if (!string.IsNullOrEmpty(workspace.WindowsPath))
                            {
                                moreCommands.Add(new CommandContextItem(new OpenInExplorerCommand(workspace.WindowsPath, workspace)));
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(workspace.WindowsPath))
                            {
                                moreCommands.Add(new CommandContextItem(new OpenInExplorerCommand(workspace.WindowsPath, workspace)));
                            }
                            moreCommands.Add(new CommandContextItem(new OpenVisualStudioCodeCommand(workspace, page, settingsManager.CommandResult, elevated: true)));
                        }
                    }
                    break;
            }

            moreCommands.Add(helpCommandContextItem);
            moreCommands.Add(new CommandContextItem(new CopyPathCommand(workspace.WindowsPath ?? string.Empty)));
            moreCommands.Add(refreshCommandContextItem);
            moreCommands.Add(new CommandContextItem(new PinWorkspaceCommand(workspace, page, workspaceStorage)));

            var item = new ListItem(command)
            {
                Title = details.Title ?? "(no title)",
                Subtitle = !string.IsNullOrEmpty(workspace.WindowsPath) ? Uri.UnescapeDataString(workspace.WindowsPath) : workspace.WindowsPath ?? string.Empty,
                Details = details,
                Icon = icon,
                Tags = tags.ToArray(),
                MoreCommands = moreCommands.ToArray(),
            };
            return item;
        }
    }
}
