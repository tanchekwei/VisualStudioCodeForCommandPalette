#pragma warning disable CA1416
using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Workspaces
{
    public static class WorkspaceFilter
    {
        public static List<VisualStudioCodeWorkspace> Filter(
            string searchText,
            List<VisualStudioCodeWorkspace> allWorkspaces,
            SearchBy searchBy,
            SortBy sortBy,
            string filterId
            )
        {
            try
            {
#if DEBUG
                using var logger = new TimeLogger();
#endif
                var isSearching = !string.IsNullOrWhiteSpace(searchText);
                var filterType = FilterType.All;
                var hasFilter = !string.IsNullOrWhiteSpace(filterId) && Enum.TryParse(filterId, out filterType) && filterType != FilterType.All;

                List<VisualStudioCodeWorkspace> pinned = new List<VisualStudioCodeWorkspace>();
                List<VisualStudioCodeWorkspace> unpinned = new List<VisualStudioCodeWorkspace>();

                if (isSearching)
                {
                    var matchedUnpinned = new List<(VisualStudioCodeWorkspace item, int score)>(allWorkspaces.Count);

                    for (int i = 0; i < allWorkspaces.Count; i++)
                    {
                        var item = allWorkspaces[i];

                        if (hasFilter && !MatchesFilter(item, filterType))
                        {
                            continue;
                        }

                        var titleScore = (searchBy is SearchBy.Title or SearchBy.Both)
                            ? FuzzyStringMatcher.ScoreFuzzy(searchText, item.Name ?? string.Empty)
                            : 0;

                        var subtitleScore = (searchBy is SearchBy.Path or SearchBy.Both)
                            ? FuzzyStringMatcher.ScoreFuzzy(searchText, item.WindowsPath ?? string.Empty)
                            : 0;

                        var bestScore = Math.Max(titleScore, subtitleScore);
                        if (bestScore > 0)
                        {
                            if (item.PinDateTime != null)
                            {
                                pinned.Add(item);
                            }
                            else
                            {
                                matchedUnpinned.Add((item, bestScore));
                            }
                        }
                    }

                    matchedUnpinned.Sort((a, b) => b.score.CompareTo(a.score));
                    for (int i = 0; i < matchedUnpinned.Count; i++)
                    {
                        unpinned.Add(matchedUnpinned[i].item);
                    }
                }
                else
                {
                    for (int i = 0; i < allWorkspaces.Count; i++)
                    {
                        var item = allWorkspaces[i];
                        if (hasFilter && !MatchesFilter(item, filterType))
                        {
                            continue;
                        }

                        if (item.PinDateTime != null)
                        {
                            pinned.Add(item);
                        }
                        else
                        {
                            unpinned.Add(item);
                        }
                    }
                }

                if (!isSearching)
                {
                    switch (sortBy)
                    {
                        case SortBy.LastAccessed:
                            unpinned.Sort((a, b) => b.LastAccessed.CompareTo(a.LastAccessed));
                            break;
                        case SortBy.Frequency:
                            unpinned.Sort((a, b) => b.Frequency.CompareTo(a.Frequency));
                            break;
                        case SortBy.RecentFromVS:
                            unpinned.Sort((a, b) => b.VSLastAccessed.CompareTo(a.VSLastAccessed));
                            break;
                        case SortBy.RecentFromVSCode:
                            // Already in the correct order from provider
                            break;
                        default:
                            unpinned.Sort((a, b) =>
                           {
                               int result = b.LastAccessed.CompareTo(a.LastAccessed);
                               if (result == 0)
                               {
                                   result = b.Frequency.CompareTo(a.Frequency);
                               }
                               return result;
                           });
                            break;
                    }
                }

                var finalItems = new List<VisualStudioCodeWorkspace>(pinned.Count + unpinned.Count);

                if (pinned.Count > 0)
                {
                    pinned.Sort((a, b) => a.PinDateTime!.Value.CompareTo(b.PinDateTime!.Value));
                    finalItems.AddRange(pinned);
                }

                finalItems.AddRange(unpinned);

                return finalItems;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return new List<VisualStudioCodeWorkspace>();
            }
        }

        private static bool MatchesFilter(VisualStudioCodeWorkspace item, FilterType filterType)
        {
            return filterType switch
            {
                FilterType.Vscode => item.VSCodeInstance?.VisualStudioCodeType == VisualStudioCodeType.Default,
                FilterType.VscodeInsider => item.VSCodeInstance?.VisualStudioCodeType == VisualStudioCodeType.Insider,
                FilterType.Cursor => item.VSCodeInstance?.VisualStudioCodeType == VisualStudioCodeType.Cursor,
                FilterType.Antigravity => item.VSCodeInstance?.VisualStudioCodeType == VisualStudioCodeType.Antigravity,
                FilterType.Windsurf => item.VSCodeInstance?.VisualStudioCodeType == VisualStudioCodeType.Windsurf,
                FilterType.Folder => item.WorkspaceType == WorkspaceType.Folder,
                FilterType.Workspace => item.WorkspaceType == WorkspaceType.Workspace,
                FilterType.RemoteCodespaces => item.VisualStudioCodeRemoteUri?.Type == VisualStudioCodeRemoteType.Codespaces,
                FilterType.RemoteWsl => item.VisualStudioCodeRemoteUri?.Type == VisualStudioCodeRemoteType.WSL,
                FilterType.RemoteDevContainer => item.VisualStudioCodeRemoteUri?.Type == VisualStudioCodeRemoteType.DevContainer,
                FilterType.RemoteAttachedContainer => item.VisualStudioCodeRemoteUri?.Type == VisualStudioCodeRemoteType.AttachedContainer,
                FilterType.RemoteSSHRemote => item.VisualStudioCodeRemoteUri?.Type == VisualStudioCodeRemoteType.SSHRemote,
                FilterType.VisualStudio2026 => item?.WorkspaceType == WorkspaceType.Solution2026,
                FilterType.VisualStudio => item?.WorkspaceType == WorkspaceType.Solution,
                _ => true,
            };
        }
    }
}
