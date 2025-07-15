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
        public static List<ListItem> Filter(string searchText, List<ListItem> allItems, SearchBy searchBy, SortBy sortBy)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            IEnumerable<ListItem> filteredItems = allItems;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var matcher = StringMatcher.Instance;
                matcher.UserSettingSearchPrecision = SearchPrecisionScore.Regular;

                filteredItems = allItems
                    .Select(item =>
                    {
                        var titleScore = (searchBy is SearchBy.Title or SearchBy.Both)
                            ? matcher.FuzzyMatch(searchText, item.Title)
                            : new MatchResult(false, 0);

                        var subtitleScore = (searchBy is SearchBy.Path or SearchBy.Both)
                            ? matcher.FuzzyMatch(searchText, item.Subtitle ?? string.Empty)
                            : new MatchResult(false, 0);

                        var bestMatch = titleScore.Score >= subtitleScore.Score ? titleScore : subtitleScore;
                        return (item, bestMatch);
                    })
                    .Where(x => x.bestMatch.Success)
                    .OrderByDescending(x => x.bestMatch.Score)
                    .Select(x => x.item);
            }

            var pinned = new List<ListItem>();
            var unpinned = new List<(ListItem Item, VisualStudioCodeWorkspace? Workspace)>();

            foreach (var item in filteredItems)
            {
                if (item.Command is IHasWorkspace { Workspace: var ws })
                {
                    if (ws?.PinDateTime is not null)
                        pinned.Add(item);
                    else
                        unpinned.Add((item, ws));
                }
                else
                {
                    unpinned.Add((item, null));
                }
            }

            var sortedUnpinned = sortBy switch
            {
                SortBy.LastAccessed => unpinned.OrderByDescending(x => x.Workspace?.LastAccessed),
                SortBy.Frequency => unpinned.OrderByDescending(x => x.Workspace?.Frequency),
                SortBy.Alphabetical => unpinned.OrderBy(x => x.Item.Title, StringComparer.OrdinalIgnoreCase),
                _ => unpinned
                    .OrderByDescending(x => x.Workspace?.LastAccessed)
                    .ThenByDescending(x => x.Workspace?.Frequency),
            };

            var finalItems = new List<ListItem>(pinned.Count + unpinned.Count);

            if (pinned.Count > 0)
            {
                finalItems.AddRange(
                    pinned.OrderBy(x => ((IHasWorkspace?)x.Command)?.Workspace?.PinDateTime)
                );
            }

            finalItems.AddRange(sortedUnpinned.Select(x => x.Item));

            foreach (var item in finalItems)
            {
                if (item.Command is IHasWorkspace { Workspace.PinDateTime: not null } &&
                    !item.Tags.Contains(WorkspaceItemFactory.PinTag))
                {
                    item.Tags = item.Tags.Append(WorkspaceItemFactory.PinTag).ToArray();
                }
            }

            return finalItems;
        }
    }
}
