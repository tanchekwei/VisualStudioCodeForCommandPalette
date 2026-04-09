// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.Indexer.Indexer;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel;
using Windows.System;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Helpers;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Pages;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models;
using WorkspaceLauncherForVSCode.Workspaces;

namespace WorkspaceLauncherForVSCode;

public sealed partial class VisualStudioCodePage : DynamicListPage, IDisposable
{
    private readonly SettingsManager _settingsManager;
    private readonly IVisualStudioCodeService _vscodeService;
    private readonly SettingsListener _settingsListener;
    private readonly WorkspaceStorage _workspaceStorage;
    private readonly IVSCodeWorkspaceWatcherService _vscodeWatcherService;
    private readonly IVSWorkspaceWatcherService _vsWatcherService;
    private readonly IPinService _pinService;

    public List<VisualStudioCodeWorkspace> AllWorkspaces { get; } = [];
    private readonly Dictionary<string, VisualStudioCodeWorkspace> _allWorkspacesById = [];
    public SettingsManager SettingsManager { get; } = new();
    public IVisualStudioCodeService VSCodeService => _vscodeService;
    private readonly List<ListItem> _visibleItems = [];
    private IListItem[]? _combinedItemsCache;
    private readonly Dictionary<string, ListItem> _listItemCache = [];
    private List<VisualStudioCodeWorkspace> _cachedFilteredWorkspaces = [];
    private string? _lastFallbackQuery;
    private List<VisualStudioCodeWorkspace> _lastFallbackFilteredWorkspaces = [];

    private readonly object _itemsLock = new();
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    private CancellationTokenSource _cancellationTokenSource = new();

    private RefreshWorkspacesCommand _refreshWorkspacesCommand;
    private CommandContextItem _refreshWorkspacesCommandContextItem;
    private CommandContextItem _helpCommandContextItem;
    private readonly IListItem[] _noResultsRefreshItem;
    private readonly IListItem[] _refreshSuggestionItem;
    private List<VisualStudioInstance> _visualStudioInstanceList = new();

    public VisualStudioCodePage
    (
        SettingsManager settingsManager,
        IVisualStudioCodeService vscodeService,
        SettingsListener settingsListener,
        WorkspaceStorage workspaceStorage,
        RefreshWorkspacesCommand refreshWorkspacesCommand,
        IVSCodeWorkspaceWatcherService vscodeWatcherService,
        IVSWorkspaceWatcherService vsWatcherService,
        IPinService pinService
    )
    {
        Name = $"Open Recent {Constant.VisualStudioCodeDisplayName}";
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        Id = $"{Package.Current.Id.Name}.{nameof(VisualStudioCodePage)}";
        var filters = new SearchFilters();
        filters.PropChanged += Filters_PropChanged;
        Filters = filters;

        _settingsManager = settingsManager;
        SettingsManager = _settingsManager;
        _vscodeService = vscodeService;
        _workspaceStorage = workspaceStorage;
        _vscodeWatcherService = vscodeWatcherService;
        _vsWatcherService = vsWatcherService;
        _pinService = pinService;
        _pinService.RefreshList += (s, e) => RefreshList();
        _vscodeWatcherService.TriggerRefresh += (s, e) => RefreshWorkspacesInBackground();
        _vsWatcherService.TriggerRefresh += (s, e) => RefreshWorkspacesInBackground();

        _helpCommandContextItem = new CommandContextItem(new HelpPage(settingsManager, null));
        _refreshWorkspacesCommand = refreshWorkspacesCommand;
        _refreshWorkspacesCommand.TriggerRefresh += (s, e) => StartRefresh();
        _refreshWorkspacesCommandContextItem = new CommandContextItem(_refreshWorkspacesCommand)
        {
            MoreCommands = [
                _helpCommandContextItem,
            ],
            RequestedShortcut = KeyChordHelpers.FromModifiers(false, false, false, false, (int)VirtualKey.F5, 0),
        };
        _settingsListener = settingsListener;
        _settingsListener.PageSettingsChanged += OnPageSettingsChanged;
        _settingsListener.SortSettingsChanged += OnSortSettingsChanged;
        _noResultsRefreshItem = [
            new ListItem(_refreshWorkspacesCommandContextItem)
            {
                Title = "No matching workspaces found",
                Subtitle = "Double-click or press Enter to refresh the list.",
            },
        ];
        _refreshSuggestionItem = [new ListItem(_refreshWorkspacesCommandContextItem)
        {
            Title = "Still not seeing what you´re looking for?",
            Subtitle = "Double-click or press Enter to refresh the list.",
        }];

        _ = RefreshWorkspacesAsync(true);
        StartStopWatchingVSCodeConfig();
    }

    private void RefreshWorkspacesInBackground()
    {
        _ = RefreshWorkspacesAsync(isUserInitiated: false, isBackground: true);
    }

    public void StartRefresh()
    {
        _ = RefreshWorkspacesAsync(isUserInitiated: true);
    }

    public override IListItem[] GetItems()
    {
        try
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            if (AllWorkspaces.Count == 0 && !IsLoading)
            {
                _ = RefreshWorkspacesAsync(isUserInitiated: false);
            }

            lock (_itemsLock)
            {
                if (_visibleItems.Count == 0 && !IsLoading && !string.IsNullOrWhiteSpace(SearchText))
                {
                    return _noResultsRefreshItem;
                }

                if (_combinedItemsCache == null)
                {
                    _combinedItemsCache = [.. _visibleItems, .. _refreshSuggestionItem];
                }

                return _combinedItemsCache;
            }
        }
        finally
        {
        }
    }

    public IListItem? GetCommandItem(string id)
    {
        if (AllWorkspaces.Count == 0 && !IsLoading)
        {
            _ = RefreshWorkspacesAsync(isUserInitiated: false);
        }

        lock (_itemsLock)
        {
            if (_allWorkspacesById.TryGetValue(id, out var workspace))
            {
                return GetOrCreateListItem(workspace, true);
            }
        }
        return null;
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
#if DEBUG
        using var logger = new TimeLogger();
        Logger.Log($"SearchText: {newSearch}");
#endif
        lock (_itemsLock)
        {
            var filterId = Filters?.CurrentFilterId ?? string.Empty;
            var isAllFilter = filterId == string.Empty || filterId == nameof(FilterType.All);

            if (isAllFilter && _lastFallbackQuery != null && _lastFallbackQuery == newSearch)
            {
                _cachedFilteredWorkspaces = _lastFallbackFilteredWorkspaces;
            }
            else
            {
#if DEBUG
                using (new TimeLogger("WorkspaceFilter.Filter (UpdateSearchText)"))
                {
                    _cachedFilteredWorkspaces = WorkspaceFilter.Filter(newSearch, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy, filterId);
                }
#else
                _cachedFilteredWorkspaces = WorkspaceFilter.Filter(newSearch, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy, filterId);
#endif
                if (isAllFilter)
                {
                    _lastFallbackQuery = newSearch;
                    _lastFallbackFilteredWorkspaces = _cachedFilteredWorkspaces;
                }
            }

            _visibleItems.Clear();
            _combinedItemsCache = null;

            List<ListItem> itemsToAdd = [];
            int count = 0;
#if DEBUG
            using (new TimeLogger($"Create {Math.Min(_cachedFilteredWorkspaces.Count, _settingsManager.PageSize)} ListItems (UpdateSearchText)"))
            {
#endif
                for (int i = 0; i < _cachedFilteredWorkspaces.Count; i++)
                {
                    if (count >= _settingsManager.PageSize) break;
                    var workspace = _cachedFilteredWorkspaces[i];
                    itemsToAdd.Add(GetOrCreateListItem(workspace));
                    count++;
                }
#if DEBUG
            }
#endif
            _visibleItems.AddRange(itemsToAdd);

            HasMoreItems = _cachedFilteredWorkspaces.Count > _settingsManager.PageSize;
        }

        RaiseItemsChanged(_visibleItems.Count);
    }

    public override void LoadMore()
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        IsLoading = true;

        lock (_itemsLock)
        {
            var currentCount = _visibleItems.Count;

            List<ListItem> itemsToAdd = [];
#if DEBUG
            int itemsToLoadCount = Math.Min(_cachedFilteredWorkspaces.Count - currentCount, _settingsManager.PageSize);
            using (new TimeLogger($"Create {itemsToLoadCount} ListItems (LoadMore)"))
            {
#endif
                for (int i = currentCount; i < _cachedFilteredWorkspaces.Count; i++)
                {
                    if (itemsToAdd.Count >= _settingsManager.PageSize) break;
                    var workspace = _cachedFilteredWorkspaces[i];
                    itemsToAdd.Add(GetOrCreateListItem(workspace));
                }
#if DEBUG
            }
#endif

            _visibleItems.AddRange(itemsToAdd);
            _combinedItemsCache = null;
            HasMoreItems = _visibleItems.Count < _cachedFilteredWorkspaces.Count;
        }

        IsLoading = false;
        RaiseItemsChanged(_visibleItems.Count);
    }

    public ListItem GetOrCreateListItem(VisualStudioCodeWorkspace workspace, bool isTopLevelCommand = false)
    {
        lock (_itemsLock)
        {
            if (!_listItemCache.TryGetValue(workspace.Id, out var listItem))
            {
                listItem = WorkspaceItemFactory.Create(workspace, this, _workspaceStorage, _settingsManager, _refreshWorkspacesCommandContextItem, _pinService, _visualStudioInstanceList);
                _listItemCache[workspace.Id] = listItem;
            }
            if (isTopLevelCommand)
            {
                var newMoreCommands = new List<ICommandContextItem>();
                if (listItem.MoreCommands != null)
                {
                    foreach (var mc in listItem.MoreCommands)
                    {
                        if (mc is CommandContextItem cci)
                        {
                            if (cci.Command is HelpPage || cci.Command is RefreshWorkspacesCommand || cci.Command is PinWorkspaceCommand)
                            {
                                continue;
                            }
                            newMoreCommands.Add(cci);
                        }
                    }
                }

                return new ListItem(listItem.Command)
                {
                    Title = listItem.Title,
                    Subtitle = listItem.Subtitle,
                    Details = listItem.Details,
                    Icon = listItem.Icon,
                    Tags = listItem.Tags,
                    MoreCommands = newMoreCommands.ToArray()
                };
            }
            return listItem;
        }
    }

    private async Task RefreshWorkspacesAsync(bool isUserInitiated, bool isBackground = false)
    {
        if (!_refreshSemaphore.Wait(0))
        {
            return;
        }

#if DEBUG
        using var totalRefreshLogger = new TimeLogger("Total RefreshWorkspacesAsync");
#endif

        try
        {
            var cancellationToken = InitializeRefresh();
#if DEBUG
            using var fetchLogger = new TimeLogger("FetchWorkspacesAsync");
#endif
            var (workspaces, solutions) = await FetchWorkspacesAsync(isUserInitiated, cancellationToken, isBackground);
#if DEBUG
            fetchLogger.Dispose();
#endif
            _visualStudioInstanceList = _vscodeService.GetVisualStudioInstances();
            if (cancellationToken.IsCancellationRequested) return;

            workspaces.AddRange(solutions);

            // Save to storage in the background to avoid blocking UI update
            _ = Task.Run(async () => await _workspaceStorage.SaveWorkspacesAsync(workspaces), CancellationToken.None);

            if (!isBackground)
            {
                FinalizeRefresh(workspaces, cancellationToken);
            }
            else
            {
                UpdateWorkspaceList(workspaces, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Task was canceled, which is expected.
        }
        finally
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                IsLoading = false;
            }
            _refreshSemaphore.Release();
        }
    }

    private CancellationToken InitializeRefresh()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        IsLoading = true;
        return _cancellationTokenSource.Token;
    }

    private async Task<(List<VisualStudioCodeWorkspace>, List<VisualStudioCodeWorkspace>)> FetchWorkspacesAsync(bool isUserInitiated, CancellationToken cancellationToken, bool isBackground = false)
    {
        if (isUserInitiated)
        {
            await _vscodeService.LoadInstancesAsync(_settingsManager.EnabledEditions, _settingsManager.CustomPath, _settingsManager.CursorPath, _settingsManager.AntigravityPath, _settingsManager.WindsurfPath);
        }

        var dbWorkspaces = await _workspaceStorage.GetWorkspacesAsync();
        var workspacesTask = _vscodeService.GetWorkspacesAsync(dbWorkspaces, cancellationToken);
        var solutionsTask = Task.FromResult<List<VisualStudioCodeWorkspace>>([]);
        if (_settingsManager.EnableVisualStudio)
        {
            solutionsTask = _vscodeService.GetVisualStudioSolutions(dbWorkspaces, true);
        }


        await Task.WhenAll(workspacesTask, solutionsTask);

        return (await workspacesTask, await solutionsTask);
    }

    private void FinalizeRefresh(List<VisualStudioCodeWorkspace> workspaces, CancellationToken cancellationToken)
    {
        UpdateWorkspaceList(workspaces, cancellationToken);
        new ToastStatusMessage($"Loaded {workspaces.Count} workspaces").Show();
    }


    private void UpdateWorkspaceList(List<VisualStudioCodeWorkspace> workspaces, CancellationToken cancellationToken)
    {
#if DEBUG
        using var logger = new TimeLogger();
        Logger.Log($"Processing {workspaces.Count} total workspaces");
#endif
        lock (_itemsLock)
        {
            if (cancellationToken.IsCancellationRequested) return;

#if DEBUG
            using (new TimeLogger("Clear and Rebuild ID Cache"))
            {
#endif
                AllWorkspaces.Clear();
                _allWorkspacesById.Clear();
                _listItemCache.Clear();
                ClearFallbackCache();
                for (int i = 0; i < workspaces.Count; i++)
                {
                    var workspace = workspaces[i];
                    if (workspace.WorkspaceType == WorkspaceType.Solution || workspace.WorkspaceType == WorkspaceType.Solution2026)
                    {
                        workspace.Id = IdGenerator.GetVisualStudioId(workspace);
                    }
                    else
                    {
                        workspace.Id = IdGenerator.GetVisualStudioCodeId(workspace);
                    }
                    AllWorkspaces.Add(workspace);
                    _allWorkspacesById[workspace.Id] = workspace;
                }
#if DEBUG
            }
#endif

            var filterId = Filters?.CurrentFilterId ?? string.Empty;
            var isAllFilter = filterId == string.Empty || filterId == nameof(FilterType.All);
            List<VisualStudioCodeWorkspace> filtered;

            if (isAllFilter && _lastFallbackQuery != null && _lastFallbackQuery == SearchText)
            {
                filtered = _lastFallbackFilteredWorkspaces;
            }
            else
            {
#if DEBUG
                using (new TimeLogger("WorkspaceFilter.Filter"))
                {
                    filtered = WorkspaceFilter.Filter(SearchText, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy, filterId);
                }
#else
                filtered = WorkspaceFilter.Filter(SearchText, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy, filterId);
#endif
                if (isAllFilter)
                {
                    _lastFallbackQuery = SearchText;
                    _lastFallbackFilteredWorkspaces = filtered;
                }
            }
            _cachedFilteredWorkspaces = filtered;
            _visibleItems.Clear();
            _combinedItemsCache = null;

            List<ListItem> itemsToAdd = [];
            int count = 0;
#if DEBUG
            using (new TimeLogger($"Create {Math.Min(filtered.Count, _settingsManager.PageSize)} ListItems"))
            {
#endif
                for (int i = 0; i < filtered.Count; i++)
                {
                    if (count >= _settingsManager.PageSize) break;
                    itemsToAdd.Add(GetOrCreateListItem(filtered[i]));
                    count++;
                }
#if DEBUG
            }
#endif
            _visibleItems.AddRange(itemsToAdd);

            HasMoreItems = _cachedFilteredWorkspaces.Count > _settingsManager.PageSize;
        }

        RaiseItemsChanged(_visibleItems.Count);
    }

    private void OnPageSettingsChanged(object? sender, EventArgs e)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        ClearFallbackCache();
        UpdateSearchText(string.Empty, SearchText);
    }

    private void OnSortSettingsChanged(object? sender, EventArgs e)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        StartStopWatchingVSCodeConfig();
        StartRefresh();
    }

    private void StartStopWatchingVSCodeConfig()
    {
        if (_settingsManager.EnableWorkspaceWatcher)
        {
            if (_settingsManager.SortBy == SortBy.RecentFromVSCode)
            {
                _vscodeWatcherService.StartWatching();
                _vsWatcherService.StopWatching();
            }
            else if (_settingsManager.SortBy == SortBy.RecentFromVS)
            {
                _vscodeWatcherService.StopWatching();
                _vsWatcherService.StartWatching();
            }
            else
            {
                _vscodeWatcherService.StopWatching();
                _vsWatcherService.StopWatching();
            }
        }
        else
        {
            _vscodeWatcherService.StopWatching();
            _vsWatcherService.StopWatching();
        }
    }

    public void RefreshList()
    {
        lock (_itemsLock)
        {
            _listItemCache.Clear();
            ClearFallbackCache();

            var filterId = Filters?.CurrentFilterId ?? string.Empty;
            _cachedFilteredWorkspaces = WorkspaceFilter.Filter(SearchText, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy, filterId);

            if (filterId == string.Empty || filterId == nameof(FilterType.All))
            {
                _lastFallbackQuery = SearchText;
                _lastFallbackFilteredWorkspaces = _cachedFilteredWorkspaces;
            }

            _visibleItems.Clear();
            _combinedItemsCache = null;

            List<ListItem> itemsToAdd = [];
            int count = 0;
            for (int i = 0; i < _cachedFilteredWorkspaces.Count; i++)
            {
                if (count >= _settingsManager.PageSize) break;
                itemsToAdd.Add(GetOrCreateListItem(_cachedFilteredWorkspaces[i]));
                count++;
            }
            _visibleItems.AddRange(itemsToAdd);

            HasMoreItems = _cachedFilteredWorkspaces.Count > _settingsManager.PageSize;
        }
        RaiseItemsChanged(_visibleItems.Count);
    }

    public async Task UpdateFrequencyAsync(string path, WorkspaceType type)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif

        await _workspaceStorage.UpdateWorkspaceFrequencyAsync(path, type);

        bool needsRefresh = false;
        lock (_itemsLock)
        {
            VisualStudioCodeWorkspace? itemToUpdate = null;
            for (int i = 0; i < AllWorkspaces.Count; i++)
            {
                var item = AllWorkspaces[i];
                if (item?.Path == path && item?.WorkspaceType == type)
                {
                    itemToUpdate = item;
                    break;
                }
            }

            if (itemToUpdate != null)
            {
                itemToUpdate.Frequency++;
                itemToUpdate.LastAccessed = DateTime.Now;
                needsRefresh = true;
            }
        }

        if (needsRefresh)
        {
            RefreshList();
        }
    }

    private void Filters_PropChanged(object sender, IPropChangedEventArgs args)
    {
        UpdateSearchText(string.Empty, SearchText);
    }

    public void ClearAllItems()
    {
        AllWorkspaces.Clear();
        _allWorkspacesById.Clear();
        _listItemCache.Clear();
        ClearFallbackCache();
    }

    public void SetSearchText(string query)
    {
        SearchText = query;
    }

    public List<VisualStudioCodeWorkspace> GetFallbackFilteredWorkspaces(string query)
    {
        lock (_itemsLock)
        {
            if (_lastFallbackQuery != null && _lastFallbackQuery == query)
            {
                return _lastFallbackFilteredWorkspaces;
            }

            _lastFallbackQuery = query;
            _lastFallbackFilteredWorkspaces = WorkspaceFilter.Filter(query, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy, nameof(FilterType.All));
            return _lastFallbackFilteredWorkspaces;
        }
    }

    private void ClearFallbackCache()
    {
        _lastFallbackQuery = null;
        _lastFallbackFilteredWorkspaces = [];
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _settingsListener.PageSettingsChanged -= OnPageSettingsChanged;
        _workspaceStorage.Dispose();
        _refreshSemaphore.Dispose();
        (_vscodeWatcherService as IDisposable)?.Dispose();
        (_vsWatcherService as IDisposable)?.Dispose();
    }
}
