// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.System;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Pages;
using WorkspaceLauncherForVSCode.Properties;
using WorkspaceLauncherForVSCode.Services;
using WorkspaceLauncherForVSCode.Workspaces;

namespace WorkspaceLauncherForVSCode;

public sealed partial class VisualStudioCodePage : DynamicListPage, IDisposable
{
    private readonly SettingsManager _settingsManager;
    private readonly IVisualStudioCodeService _vscodeService;
    private readonly SettingsListener _settingsListener;
    private readonly WorkspaceStorage _workspaceStorage;
    private readonly CountTracker _countTracker;
    private readonly IVSCodeWorkspaceWatcherService _vscodeWatcherService;
    private readonly IVSWorkspaceWatcherService _vsWatcherService;
    private readonly IPinService _pinService;

    public List<VisualStudioCodeWorkspace> AllWorkspaces { get; } = new();
    public SettingsManager SettingsManager { get; } = new();
    private readonly List<ListItem> _visibleItems = new();
    private List<VisualStudioCodeWorkspace> _cachedFilteredWorkspaces = new();

    private readonly object _itemsLock = new();
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    private CancellationTokenSource _cancellationTokenSource = new();

    private RefreshWorkspacesCommand _refreshWorkspacesCommand;
    private CommandContextItem _refreshWorkspacesCommandContextItem;
    private CommandContextItem _helpCommandContextItem;
    private readonly IListItem[] _noResultsRefreshItem;
    private readonly IListItem[] _refreshSuggestionItem;

    public VisualStudioCodePage
    (
        SettingsManager settingsManager,
        IVisualStudioCodeService vscodeService,
        SettingsListener settingsListener,
        WorkspaceStorage workspaceStorage,
        RefreshWorkspacesCommand refreshWorkspacesCommand,
        CountTracker countTracker,
        IVSCodeWorkspaceWatcherService vscodeWatcherService,
        IVSWorkspaceWatcherService vsWatcherService,
        IPinService pinService
    )
    {
        Title = Resource.page_title;
#if DEBUG
        Title += " (Dev)";
#endif
        Icon = Classes.Icon.VisualStudioAndVisualStudioCode;
        Name = Resource.page_command_name;
        Id = "VisualStudioCodePage";

        _settingsManager = settingsManager;
        SettingsManager = _settingsManager;
        _vscodeService = vscodeService;
        _workspaceStorage = workspaceStorage;
        _countTracker = countTracker;
        _vscodeWatcherService = vscodeWatcherService;
        _vsWatcherService = vsWatcherService;
        _pinService = pinService;
        _pinService.RefreshList += (s, e) => RefreshList();
        _vscodeWatcherService.TriggerRefresh += (s, e) => RefreshWorkspacesInBackground();
        _vsWatcherService.TriggerRefresh += (s, e) => RefreshWorkspacesInBackground();

        _helpCommandContextItem = new CommandContextItem(new HelpPage(settingsManager, countTracker, null));
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
                RefreshWorkspacesAsync(isUserInitiated: false).GetAwaiter().GetResult();
            }

            lock (_itemsLock)
            {
                if (_visibleItems.Count == 0 && !IsLoading && !string.IsNullOrWhiteSpace(SearchText))
                {
                    return _noResultsRefreshItem;
                }
                return _visibleItems.Concat(_refreshSuggestionItem).ToArray();
            }
        }
        finally
        {
        }
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (oldSearch == newSearch)
        {
            return;
        }
#if DEBUG
        using var logger = new TimeLogger();
        Logger.Log($"SearchText: {newSearch}");
#endif
        lock (_itemsLock)
        {
            _cachedFilteredWorkspaces = WorkspaceFilter.Filter(newSearch, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy);
            _visibleItems.Clear();
            _visibleItems.AddRange(CreateListItems(_cachedFilteredWorkspaces.Take(_settingsManager.PageSize)));
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
            var nextPage = _cachedFilteredWorkspaces
                .Skip(currentCount)
                .Take(_settingsManager.PageSize);

            _visibleItems.AddRange(CreateListItems(nextPage));
            HasMoreItems = _visibleItems.Count < _cachedFilteredWorkspaces.Count;
        }

        IsLoading = false;
        RaiseItemsChanged(_visibleItems.Count);
    }

    private IEnumerable<ListItem> CreateListItems(IEnumerable<VisualStudioCodeWorkspace> workspaces)
    {
        return workspaces.Select(w =>
            WorkspaceItemFactory.Create(w, this, _workspaceStorage, _settingsManager, _countTracker, _refreshWorkspacesCommandContextItem, _pinService));
    }

    private async Task RefreshWorkspacesAsync(bool isUserInitiated, bool isBackground = false)
    {
        if (!_refreshSemaphore.Wait(0))
        {
            return;
        }

        try
        {
            var cancellationToken = InitializeRefresh();
            var (workspaces, solutions) = await FetchWorkspacesAsync(isUserInitiated, cancellationToken, isBackground);

            if (cancellationToken.IsCancellationRequested) return;

            var allWorkspaces = await ProcessAndSaveWorkspaces(workspaces, solutions);
            if (!isBackground)
            {
                FinalizeRefresh(allWorkspaces, cancellationToken);
            }
            else
            {
                UpdateWorkspaceList(allWorkspaces, cancellationToken);
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
        _countTracker.Reset();
        return _cancellationTokenSource.Token;
    }

    private async Task<(List<VisualStudioCodeWorkspace>, List<VisualStudioCodeWorkspace>)> FetchWorkspacesAsync(bool isUserInitiated, CancellationToken cancellationToken, bool isBackground = false)
    {
        if (isUserInitiated)
        {
            _vscodeService.LoadInstances(_settingsManager.EnabledEditions);
        }

        var dbWorkspaces = await _workspaceStorage.GetWorkspacesAsync();
        var workspacesTask = _vscodeService.GetWorkspacesAsync(dbWorkspaces, cancellationToken);
        var solutionsTask = Task.FromResult(new List<VisualStudioCodeWorkspace>());
        if (_settingsManager.EnableVisualStudio)
        {
            solutionsTask = _vscodeService.GetVisualStudioSolutions(dbWorkspaces, true);
        }


        await Task.WhenAll(workspacesTask, solutionsTask);

        return (await workspacesTask, await solutionsTask);
    }

    private async Task<List<VisualStudioCodeWorkspace>> ProcessAndSaveWorkspaces(List<VisualStudioCodeWorkspace> workspaces, List<VisualStudioCodeWorkspace> solutions)
    {
        _countTracker.Update(CountType.VisualStudioCode, workspaces.Count);
        _countTracker.Update(CountType.VisualStudio, solutions.Count);

        workspaces.AddRange(solutions);

        _countTracker.Update(CountType.Total, workspaces.Count);

        await _workspaceStorage.SaveWorkspacesAsync(workspaces);
        return workspaces;
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
#endif
        lock (_itemsLock)
        {
            if (cancellationToken.IsCancellationRequested) return;

            AllWorkspaces.Clear();
            AllWorkspaces.AddRange(workspaces);

            var filtered = WorkspaceFilter.Filter(SearchText, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy);
            _cachedFilteredWorkspaces = filtered;
            _visibleItems.Clear();
            _visibleItems.AddRange(CreateListItems(filtered.Take(_settingsManager.PageSize)));
            HasMoreItems = _cachedFilteredWorkspaces.Count > _settingsManager.PageSize;
        }

        RaiseItemsChanged(_visibleItems.Count);
    }

    private void OnPageSettingsChanged(object? sender, EventArgs e)
    {
#if DEBUG
            using var logger = new TimeLogger();
#endif
        UpdateSearchText(SearchText, SearchText);
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
            if (_settingsManager.SortBy == Enums.SortBy.RecentFromVSCode)
            {
                _vscodeWatcherService.StartWatching();
                _vsWatcherService.StopWatching();
            }
            else if (_settingsManager.SortBy == Enums.SortBy.RecentFromVS)
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
            _cachedFilteredWorkspaces = WorkspaceFilter.Filter(SearchText, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy);
            _visibleItems.Clear();
            _visibleItems.AddRange(CreateListItems(_cachedFilteredWorkspaces.Take(_settingsManager.PageSize)));
            HasMoreItems = _cachedFilteredWorkspaces.Count > _settingsManager.PageSize;
        }
        RaiseItemsChanged(_visibleItems.Count);
    }

    public async Task UpdateFrequencyAsync(string path)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif

        await _workspaceStorage.UpdateWorkspaceFrequencyAsync(path);

        lock (_itemsLock)
        {
            var itemToUpdate = AllWorkspaces.FirstOrDefault(item => item?.Path == path);
            if (itemToUpdate != null)
            {
                if (itemToUpdate != null)
                {
                    itemToUpdate.Frequency++;
                    itemToUpdate.LastAccessed = DateTime.Now;
                }

                // Re-apply filter and sort
                _cachedFilteredWorkspaces = WorkspaceFilter.Filter(SearchText, AllWorkspaces, _settingsManager.SearchBy, _settingsManager.SortBy);
                _visibleItems.Clear();
                _visibleItems.AddRange(CreateListItems(_cachedFilteredWorkspaces.Take(_settingsManager.PageSize)));
                HasMoreItems = _cachedFilteredWorkspaces.Count > _settingsManager.PageSize;
            }
        }
    }

    public void ClearAllItems()
    {
        AllWorkspaces.Clear();
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
