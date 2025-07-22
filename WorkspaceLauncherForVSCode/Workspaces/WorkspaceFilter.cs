#pragma warning disable CA1416
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;

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
                IEnumerable<VisualStudioCodeWorkspace> filteredItems = allWorkspaces;
                var isSearching = !string.IsNullOrWhiteSpace(searchText);

                if (isSearching)
                {
                    var matcher = StringMatcher.Instance;
                    matcher.UserSettingSearchPrecision = SearchPrecisionScore.Regular;

                    filteredItems = allWorkspaces
                        .Select(item =>
                        {
                            var titleScore = (searchBy is SearchBy.Title or SearchBy.Both)
                                ? matcher.FuzzyMatch(searchText, item.Name ?? string.Empty)
                                : new MatchResult(false, 0);

                            var subtitleScore = (searchBy is SearchBy.Path or SearchBy.Both)
                                ? matcher.FuzzyMatch(searchText, item.WindowsPath ?? string.Empty)
                                : new MatchResult(false, 0);

                            var bestMatch = titleScore.Score >= subtitleScore.Score ? titleScore : subtitleScore;
                            return (item, bestMatch);
                        })
                        .Where(x => x.bestMatch.Success)
                        .OrderByDescending(x => x.bestMatch.Score)
                        .Select(x => x.item);
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

                IEnumerable<VisualStudioCodeWorkspace>? sortedUnpinned;
                if (isSearching)
                {
                    sortedUnpinned = unpinned
                            .OrderByDescending(x => x.LastAccessed)
                            .ThenByDescending(x => x.Frequency);
                }
                else
                {
                    sortedUnpinned = sortBy switch
                    {
                        SortBy.LastAccessed => unpinned.OrderByDescending(x => x.LastAccessed),
                        SortBy.Frequency => unpinned.OrderByDescending(x => x.Frequency),
                        SortBy.RecentFromVSCode => unpinned.AsEnumerable(),
                        SortBy.RecentFromVS => unpinned.OrderByDescending(x => x.VSLastAccessed),
                        _ => unpinned
                            .OrderByDescending(x => x.LastAccessed)
                            .ThenByDescending(x => x.Frequency),
                    };
                }

                var finalItems = new List<VisualStudioCodeWorkspace>(pinned.Count + unpinned.Count);

                if (pinned.Count > 0)
                {
                    finalItems.AddRange(pinned.OrderBy(x => x.PinDateTime));
                }

                finalItems.AddRange(sortedUnpinned);

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
