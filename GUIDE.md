# Project Guide: Workspace Launcher for Visual Studio & VS Code

This document provides an overview of the project's architecture, focusing on its design principles and key components.

## Architecture Overview

The extension is designed following SOLID principles to ensure it is maintainable, scalable, and testable.

### Key Principles

*   **Single Responsibility Principle (SRP):** Each class has a single, well-defined responsibility. This is exemplified by the service layer, where responsibilities are highly granular. For instance, `VisualStudioCodeInstanceProvider` is solely responsible for discovering Visual Studio Code installations, while `VisualStudioProvider` is responsible for discovering Visual Studio solutions.
*   **Dependency Inversion Principle (DIP):** High-level modules do not depend on low-level modules; both depend on abstractions. This is achieved through the use of interfaces like `IVisualStudioCodeService`. The `WorkspaceLauncherForVSCodeCommandsProvider` and `VisualStudioCodePage` depend on the `IVisualStudioCodeService` interface, not the concrete `VisualStudioCodeService` implementation. This decoupling allows for easier testing and maintenance.

## Project Structure

The project is organized into the following key directories within the `WorkspaceLauncherForVSCode` project:

- **`WorkspaceLauncherForVSCode/`**:
  - [`WorkspaceLauncherForVSCode.cs`](./WorkspaceLauncherForVSCode/WorkspaceLauncherForVSCode.cs) - The main extension implementation and Dependency Injection container setup.
  - [`WorkspaceLauncherForVSCodeCommandsProvider.cs`](./WorkspaceLauncherForVSCode/WorkspaceLauncherForVSCodeCommandsProvider.cs) - The entry point for providing commands and fallback items to the Command Palette.
  - [`FallbackOpenRecentVisualStudioCodeItem.cs`](./WorkspaceLauncherForVSCode/FallbackOpenRecentVisualStudioCodeItem.cs) - A fallback command item that allows searching and jumping to the extension's page.
- **`/Classes`**: Contains core data models and helper classes.
  - [`SettingsManager.cs`](./WorkspaceLauncherForVSCode/Classes/SettingsManager.cs) - Manages user-configurable settings using the Command Palette's settings API.
  - `VisualStudioCodeInstance.cs`, `VisualStudioCodeWorkspace.cs` - Core data models representing editor installations and workspaces/solutions.
  - `WorkspaceStorage.cs` - Manages persistence of workspace history, usage frequency, and manual pinning.
  - `Constant.cs` - Centralized constants for the project.
- **`/Commands`**: Contains command implementations for various actions.
    - `OpenVisualStudioCodeCommand.cs` - Launches a selected VS Code (or Cursor/Windsurf/Antigravity) workspace.
    - `OpenSolutionCommand.cs` - Launches a selected Visual Studio solution.
    - `OpenInTerminalCommand.cs` - Opens the workspace location in the configured terminal.
    - `OpenInExplorerCommand.cs` - Opens the folder in File Explorer.
    - `PinWorkspaceCommand.cs` - (Un)pins a workspace to the list.
    - `RefreshWorkspacesCommand.cs` - Manually triggers a workspace list refresh.
    - `SwitchWindowCommand.cs` - Switches to an existing window if the solution is already open.
    - `ResetFrequencyCommand.cs` - Resets the usage frequency of a workspace.
    - `OpenUrlCommand.cs` - Opens a URL (used for Codespaces).
- **`/Components`**: Contains window management logic (WindowWalker integration).
    - `OpenWindows.cs`, `Window.cs`, `WindowProcess.cs` - Enumerate and manage open application windows to support window switching.
- **`/Enums`**: Defines various enumerations used throughout the app (e.g., `WorkspaceType`, `VisualStudioCodeEdition`, `SortBy`, `FilterType`).
- **`/Helpers`**: Contains utility classes.
  - `IdGenerator.cs` - Utility for generating unique, stable IDs for workspaces and solutions.
  - `NativeMethods.cs`, `WslPathHelper.cs`, `FileUriParser.cs` - System-level integration and path parsing utilities.
- **`/Interfaces`**: Defines service contracts (`IVisualStudioCodeService`, `IPinService`, `IVSCodeWorkspaceWatcherService`, etc.).
- **`/Listeners`**:
  - [`SettingsListener.cs`](./WorkspaceLauncherForVSCode/Listeners/SettingsListener.cs) - Monitors for setting changes to trigger UI updates and re-discovery.
- **`/Pages`**: Contains UI components.
  - [`VisualStudioCodePage.cs`](./WorkspaceLauncherForVSCode/Pages/VisualStudioCodePage.cs) - The main dynamic list page with search, filtering, and caching.
  - [`FallbackWorkspaceItem.cs`](./WorkspaceLauncherForVSCode/Pages/FallbackWorkspaceItem.cs) - Individual fallback results that appear in the main Command Palette search.
  - `HelpPage.cs` & `StaticHelpItems.cs` - Provides contextual help and shortcuts documentation.
  - `DetailPage.cs` - Provides a detailed view for a selected workspace.
- **`/Services`**: Primary service implementations.
  - [`VisualStudioCodeService.cs`](./WorkspaceLauncherForVSCode/Services/VisualStudioCodeService.cs) - Orchestrates instance and workspace discovery.
  - `VisualStudioCodeInstanceProvider.cs` - Scans for editor installations (VS Code, Cursor, etc.).
  - `VisualStudioCodeWorkspaceProvider.cs` - Finds recent VS Code workspaces via internal readers.
  - `VisualStudioProvider.cs` - Discovers Visual Studio solutions.
  - `PinService.cs` - Manages workspace pinning logic.
  - `VSWorkspaceWatcherService.cs` & `VSCodeWorkspaceWatcherService.cs` - File system watchers for real-time updates when recent project files change.
  - **`/Services/VisualStudio`**: Core logic for Visual Studio version and solution discovery.
    - `VisualStudioService.cs` - Primary service for interacting with Visual Studio installations.
- **`/Workspaces`**: Workspace-specific logic.
    - `WorkspaceFilter.cs` - Implements searching, filtering (by type/remote), and sorting logic.
    - [`WorkspaceItemFactory.cs`](./WorkspaceLauncherForVSCode/Workspaces/WorkspaceItemFactory.cs) - Constructs UI `ListItem` objects from workspace models.
    - `VscdbDatabase.cs` - Handles SQLite interaction for VS Code's `state.vscdb`.
    - **`/Readers`**: Parsers for editor-specific metadata (SQLite and JSON).

## Core Components and Features

### Performance and Caching

To ensure the extension remains responsive even with hundreds of projects:
- **ListItem Caching**: `VisualStudioCodePage` maintains a cache of `ListItem` objects to avoid re-constructing them during search or scroll.
- **Search Caching**: Results of the last search query are cached and reused by fallback items to prevent redundant filtering operations.
- **Asynchronous Loading**: All I/O and process execution (like `vswhere.exe`) are handled off the UI thread.
- **Native AOT**: The project is optimized for Ahead-of-Time compilation, resulting in faster startup and lower memory usage.

### Search and Fallback Integration

The extension integrates deeply with the Command Palette through **Fallback Commands**:
- **Direct Results**: A configurable number of top-matching workspaces are injected directly into the main Command Palette search list via `FallbackWorkspaceItem`. This allows users to launch their most relevant projects instantly from the global search.
- **Search-to-Page**: The `FallbackOpenRecentVisualStudioCodeItem` provides a "Search for..." command that opens the extension's dedicated page pre-filtered with the current query.
- **Advanced Filtering**: Users can filter the list by workspace type (Local, WSL, SSH, Dev Container) or editor type via the UI filters.

### Pinning and Persistence

The `PinService` and `WorkspaceStorage` work together to manage project metadata:
- **Advanced Pinning**: Supports **Pin to List**, **Pin to Home**, and **Pin to Dock**. Pinned projects are highlighted with a tag and remain easily accessible across sessions. The extension integrates with Command Palette's pinning system by providing stable identifiers.
- **Frequency Tracking**: The extension tracks how often projects are opened through it, allowing for "Usage Count" sorting.
- **External Recent Lists**: The extension can also sort items to match the internal "Open Recent" order of VS Code or Visual Studio by reading their respective metadata.

### Visual Studio & Editor Support

- **Broad Compatibility**: Supports VS Code (User/System/Insiders), Cursor, Windsurf, and Google Antigravity.
- **Visual Studio 2026**: Preliminary support for the next version of Visual Studio.
- **Helper Launcher**: An optional `VisualStudioCodeForCommandPaletteLauncher.exe` is used to ensure editors come to the foreground when launched from the background extension process.
- **Window Switching**: Uses window enumeration to detect already-open solutions and switches focus instead of opening duplicate instances.

### UI and Contextual Actions

- **Rich Details**: Workspaces provide a details panel (built via `WorkspaceItemFactory`) that provides a rich preview of the project, including its type, path, and metadata.
- **Contextual Commands**: Every item supports a suite of secondary actions:
    - Open as Administrator.
    - Open in Explorer / Terminal.
    - Copy Path.
    - Contextual Help (F1).
- **Shortcuts**: Common actions support keyboard shortcuts (e.g., Ctrl+C for Copy Path, F5 for Refresh).

This architecture ensures a clean separation of concerns, making the codebase easier to understand, extend, and debug.