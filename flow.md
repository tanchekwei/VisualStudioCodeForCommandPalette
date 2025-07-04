# Application Flow

This document describes the sequence of operations within the Workspace Launcher extension for the PowerToys Command Palette, based on a detailed analysis of the source code.

## 1. Extension Initialization

This flow is triggered when the Command Palette host process loads the extension. The design prioritizes a fast startup by loading cached data from a local SQLite database first, followed by a background refresh.

1.  **Entry Point**: `Program.Main` creates an instance of `WorkspaceLauncherForVSCode`.
2.  **Extension Core**: The `WorkspaceLauncherForVSCode` class constructor instantiates `WorkspaceLauncherForVSCodeCommandsProvider`.
3.  **Command Provider Initialization**: The `WorkspaceLauncherForVSCodeCommandsProvider` constructor is the central point of initialization:
    *   It creates a `SettingsManager` to handle user settings.
    *   It creates the `VisualStudioCodeService`, the main service for data retrieval.
    *   It calls `_vscodeService.LoadInstances()` to discover all installed VS Code versions.
    *   Crucially, it creates the `VisualStudioCodePage`, which handles the UI and data presentation.
4.  **UI and Data Loading**: The `VisualStudioCodePage` constructor kicks off the data loading process by calling `LoadInitialWorkspacesAsync()`.
5.  **Initial Database Cache Load**: `LoadInitialWorkspacesAsync()` performs the following steps:
    *   It calls `_workspaceStorage.GetWorkspacesAsync()` which reads the locally cached list of workspaces from a SQLite database file named `workspaces.db`.
    *   If the cache database is not empty, it immediately calls `UpdateWorkspaceList()` to populate the UI with this stored data. This ensures the user sees a list of workspaces almost instantly.
6.  **Asynchronous Background Refresh**:
    *   After loading from the database cache, `LoadInitialWorkspacesAsync()` calls `RefreshWorkspacesAsync()`.
    *   This method fetches the latest workspace and solution data in the background using `_vscodeService.GetWorkspacesAsync()` and `_vscodeService.GetVisualStudioSolutions()`.
    *   This involves reading from the live VS Code data sources (`state.vscdb` and `storage.json`) and Visual Studio's `CodeContainers.json`.
    *   Once the fresh data is retrieved, it's saved back to the `workspaces.db` cache via `_workspaceStorage.SaveWorkspacesAsync()`.
    *   Finally, `UpdateWorkspaceList()` is called again to update the UI with the fresh, complete list of items.

## 2. Extension Activation

This flow occurs when the user opens the Command Palette and invokes the extension.

1.  **`GetItems()`**: The Command Palette calls the `GetItems()` method on the `VisualStudioCodePage`.
2.  **Display Items**: This method returns the `_visibleItems` collection, which contains the list of workspaces/solutions that were loaded from the `workspaces.db` cache and potentially updated by the background refresh.

## 3. User Searches for a Workspace

This flow describes the real-time filtering that happens as a user types in the search bar.

1.  **`UpdateSearchText()`**: With each keystroke, this method is called with the old and new search text.
2.  **In-Memory Filtering**:
    *   The method calls `WorkspaceFilter.Filter()`, which performs a fast, in-memory search over the complete list of items (`_allItems`).
    *   This filtering does **not** involve any I/O operations.
3.  **UI Update**:
    *   The results of the filter are placed into the `_visibleItems` list.
    *   `RaiseItemsChanged()` is called, which signals the Command Palette to request the updated list via `GetItems()`, causing the UI to refresh with the filtered results.