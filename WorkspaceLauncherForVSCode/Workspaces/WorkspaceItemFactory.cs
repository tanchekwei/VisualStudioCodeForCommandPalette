// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.System;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Helpers;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Pages;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models;

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
            CommandContextItem refreshCommandContextItem,
            IPinService pinService,
            List<VisualStudioInstance> visualStudioInstanceList)
        {
            try
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
                    case WorkspaceType.Solution2026:
                        command = new OpenSolutionCommand(workspace, page);
                        icon = Icon.VisualStudio;
                        if (workspace.WorkspaceType == WorkspaceType.Solution2026 || workspace.VSInstance?.ProductLineVersion == Constant.VisualStudio2026Version)
                        {
                            icon = Icon.VisualStudio2026;
                        }
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

                        // Add alternative version opening options based on installed instances
                        foreach (var instance in visualStudioInstanceList)
                        {
                            if (workspace.VSInstance != null &&
                                string.Equals(workspace.VSInstance.InstancePath, instance.InstancePath, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            var altIcon = Icon.VisualStudio;
                            var versionLabel = instance.ProductLineVersion;
                            var targetType = WorkspaceType.Solution;

                            if (instance.ProductLineVersion == Constant.VisualStudio2026Version) // "18"
                            {
                                altIcon = Icon.VisualStudio2026;
                                versionLabel = "2026";
                                targetType = WorkspaceType.Solution2026;
                            }

                            var tempWorkspace = new VisualStudioCodeWorkspace
                            {
                                Path = workspace.Path,
                                Name = workspace.Name,
                                WindowsPath = workspace.WindowsPath,
                                WorkspaceType = targetType,
                                VSInstance = instance,
                                VisualStudioCodeRemoteUri = workspace.VisualStudioCodeRemoteUri
                            };

                            var altCommand = new OpenSolutionCommand(tempWorkspace, page, elevated: false)
                            {
                                Name = $"Open in Visual Studio {versionLabel}",
                                Icon = altIcon
                            };
                            moreCommands.Add(new CommandContextItem(altCommand));
                        }

                        break;
                    default:
                        command = new OpenVisualStudioCodeCommand(workspace, page);
                        icon = Icon.VisualStudioCode;
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
                            switch (workspace.VisualStudioCodeRemoteUri.Type)
                            {
                                case VisualStudioCodeRemoteType.Codespaces:
                                    if (!string.IsNullOrEmpty(workspace.WindowsPath))
                                    {
                                        moreCommands.Add(new CommandContextItem(new Commands.OpenUrlCommand(workspace.WindowsPath, "Open in Browser", Icon.GitHub)));
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

                bool hasOpenInExplorer = false;
                foreach (var c in moreCommands)
                {
                    if (c.Command is OpenInExplorerCommand)
                    {
                        hasOpenInExplorer = true;
                        break;
                    }
                }
                if (hasOpenInExplorer)
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
                moreCommands.Add(new CommandContextItem(new PinWorkspaceCommand(workspace, pinService)));

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

                if (item.Command is IHasWorkspace { Workspace.PinDateTime: not null })
                {
                    bool hasPinTag = false;
                    foreach (var tag in item.Tags)
                    {
                        if (tag == PinTag)
                        {
                            hasPinTag = true;
                            break;
                        }
                    }

                    if (!hasPinTag)
                    {
                        var newTags = new Tag[item.Tags.Length + 1];
                        item.Tags.CopyTo(newTags, 0);
                        newTags[item.Tags.Length] = PinTag;
                        item.Tags = newTags;
                    }
                }
                return item;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return new ListItem(new OpenVisualStudioCodeCommand(workspace, page));
            }
        }
    }
}

