// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.System;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Helpers;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Pages;

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
            VisualStudioCodePage page,
            WorkspaceStorage workspaceStorage,
            SettingsManager settingsManager,
            CountTracker countTracker,
            CommandContextItem refreshCommandContextItem)
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
                    command = new OpenSolutionCommand(workspace, page);
                    icon = Classes.Icon.VisualStudio;
                    workspace.WindowsPath = workspace.Path;
                    details = new Details
                    {
                        Title = workspace.Name ?? string.Empty,
                        HeroImage = icon,
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
                        moreCommands.Add(new CommandContextItem(new OpenSolutionCommand(workspace, page, elevated: true)));
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
                        moreCommands.Add(new CommandContextItem(new OpenSolutionCommand(workspace, page, elevated: true)));
                    }
                    break;
                default:
                    command = new OpenVisualStudioCodeCommand(workspace, page);
                    icon = Classes.Icon.VisualStudioCode;
                    details = new Details
                    {
                        Title = workspace.GetWorkspaceName(),
                        HeroImage = icon,
                    };
                    if (settingsManager.TagTypes.HasFlag(TagType.Type))
                    {
                        if (workspace.VisualStudioCodeRemoteUri != null)
                        {
                            tags.Add(new Tag(workspace.VisualStudioCodeRemoteUri.Type.ToDisplayName()));
                        }
                        else if (workspace.VisualStudioCodeRemoteUri?.TypeStr != null)
                        {
                            tags.Add(new Tag(workspace.VisualStudioCodeRemoteUri.TypeStr));
                        }
                        else if (workspace.WorkspaceType == WorkspaceType.Workspace)
                        {
                            tags.Add(new Tag(nameof(WorkspaceType.Workspace)));
                        }
                    }
                    if (settingsManager.TagTypes.HasFlag(TagType.Target))
                    {
                        if (workspace.VSCodeInstance?.Name is string name)
                        {
                            tags.Add(new Tag(name));
                        }
                    }
                    if (workspace.VisualStudioCodeRemoteUri is null)
                    {
                        countTracker.Update((WorkspaceType)workspace.WorkspaceType, countTracker[(WorkspaceType)workspace.WorkspaceType] + 1);
                        if (settingsManager.VSCodeSecondaryCommand == SecondaryCommand.OpenAsAdministrator)
                        {
                            moreCommands.Add(new CommandContextItem(new OpenVisualStudioCodeCommand(workspace, page, elevated: true)));
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
                            moreCommands.Add(new CommandContextItem(new OpenVisualStudioCodeCommand(workspace, page, elevated: true)));
                        }
                    }
                    else
                    {
                        if (workspace.VisualStudioCodeRemoteUri.Type.HasValue)
                        {
                            countTracker.Update(workspace.VisualStudioCodeRemoteUri.Type.Value, countTracker[workspace.VisualStudioCodeRemoteUri.Type.Value] + 1);
                        }
                        switch (workspace.VisualStudioCodeRemoteUri.Type)
                        {
                            case VisualStudioCodeRemoteType.Codespaces:
                                if (!string.IsNullOrEmpty(workspace.WindowsPath))
                                {
                                    moreCommands.Add(new CommandContextItem(new Commands.OpenUrlCommand(workspace.WindowsPath, "Open in Browser", Classes.Icon.GitHub)));
                                }
                                break;
                            case VisualStudioCodeRemoteType.WSL:
                            case VisualStudioCodeRemoteType.DevContainer:
                                if (!string.IsNullOrEmpty(workspace.WindowsPath))
                                {
                                    moreCommands.Add(new CommandContextItem(new OpenInExplorerCommand(workspace.WindowsPath, workspace)));
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }

            if (moreCommands.Any(c => c.Command is OpenInExplorerCommand))
            {
                moreCommands.Add(new CommandContextItem(new OpenInTerminalCommand(workspace, settingsManager)));
            }

            moreCommands.Add(new CommandContextItem(new HelpPage(settingsManager, countTracker, workspace))
            {
                RequestedShortcut = KeyChordHelpers.FromModifiers(false, false, false, false, (int)VirtualKey.F1, 0),
            });
            moreCommands.Add(new CommandContextItem(new CopyPathCommand(workspace.WindowsPath ?? string.Empty))
            {
                RequestedShortcut = KeyChordHelpers.FromModifiers(true, false, false, false, (int)VirtualKey.C, 0),
            });
            moreCommands.Add(refreshCommandContextItem);
            moreCommands.Add(new CommandContextItem(new PinWorkspaceCommand(workspace, page, workspaceStorage)));

            string subtitle = string.Empty;
            if (workspace.VisualStudioCodeRemoteUri is null)
            {
                subtitle = Uri.UnescapeDataString(workspace.WindowsPath ?? string.Empty);
            }
            else
            {
                subtitle = workspace.WindowsPath ?? string.Empty;
            }

            var item = new ListItem(command)
            {
                Title = details.Title ?? "(no title)",
                Subtitle = subtitle ?? string.Empty,
                Details = details,
                Icon = icon,
                Tags = tags.ToArray(),
                MoreCommands = moreCommands.ToArray(),
            };

            if (item.Command is IHasWorkspace { Workspace.PinDateTime: not null } &&
                !item.Tags.Contains(PinTag))
            {
                item.Tags = item.Tags.Append(PinTag).ToArray();
            }
            return item;
        }
    }
}
