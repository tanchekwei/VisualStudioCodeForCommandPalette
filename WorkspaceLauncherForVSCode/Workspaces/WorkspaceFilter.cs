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
            SortBy sortBy)
        {
            try
            {
#if DEBUG
                using var logger = new TimeLogger();
#endif
                List<VisualStudioCodeWorkspace> filteredItems;
                var isSearching = !string.IsNullOrWhiteSpace(searchText);

                if (isSearching)
                {
                    var matchedItems = new List<(VisualStudioCodeWorkspace item, int score)>(allWorkspaces.Count);

                    foreach (var item in allWorkspaces)
                    {
                        var titleScore = (searchBy is SearchBy.Title or SearchBy.Both)
                            ? FuzzyStringMatcher.ScoreFuzzy(searchText, item.Name ?? string.Empty)
                            : 0;

                        var subtitleScore = (searchBy is SearchBy.Path or SearchBy.Both)
                            ? FuzzyStringMatcher.ScoreFuzzy(searchText, item.WindowsPath ?? string.Empty)
                            : 0;

                        var bestScore = Math.Max(titleScore, subtitleScore);
                        if (bestScore > 0)
                        {
                            matchedItems.Add((item, bestScore));
                        }
                    }

                    matchedItems.Sort((a, b) => b.score.CompareTo(a.score));

                    filteredItems = new List<VisualStudioCodeWorkspace>(matchedItems.Count);
                    foreach (var match in matchedItems)
                    {
                        filteredItems.Add(match.item);
                    }
                }
                else
                {
                    filteredItems = new List<VisualStudioCodeWorkspace>(allWorkspaces);
                }

                var pinned = new List<VisualStudioCodeWorkspace>();
                var unpinned = new List<VisualStudioCodeWorkspace>();

                foreach (var item in filteredItems)
                {
                    if (item.PinDateTime != null)
                    {
                        pinned.Add(item);
                    }
                    else
                    {
                        unpinned.Add(item);
                    }
                }

                if (isSearching)
                {
                    unpinned.Sort((a, b) =>
                    {
                        int result = b.LastAccessed.CompareTo(a.LastAccessed);
                        if (result == 0)
                        {
                            result = b.Frequency.CompareTo(a.Frequency);
                        }
                        return result;
                    });
                }
                else
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
    }
}
