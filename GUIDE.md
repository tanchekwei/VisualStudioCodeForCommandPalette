# Project Guide: Workspace Launcher for Visual Studio & VS Code

This document provides an overview of the project's architecture, focusing on its design principles and key components.

## Architecture Overview

The extension is designed following SOLID principles to ensure it is maintainable, scalable, and testable.

### Key Principles

*   **Single Responsibility Principle (SRP):** Each class has a single, well-defined responsibility. This is exemplified by the service layer, where responsibilities are highly granular. For instance, `VisualStudioCodeInstanceProvider` is solely responsible for discovering Visual Studio Code installations, while `VisualStudioProvider` is responsible for discovering Visual Studio solutions.
*   **Dependency Inversion Principle (DIP):** High-level modules do not depend on low-level modules; both depend on abstractions. This is achieved through the use of interfaces like `IVisualStudioCodeService`. The `WorkspaceLauncherForVSCodeCommandsProvider` and `VisualStudioCodePage` depend on the `IVisualStudioCodeService` interface, not the concrete `VisualStudioCodeService` implementation. This decoupling allows for easier testing and maintenance.

## Project Structure

The project is organized into the following key directories within the `WorkspaceLauncherForVSCode` project:

- **`/`**:
  - [`WorkspaceLauncherForVSCode.cs`](./WorkspaceLauncherForVSCode.cs) - The main extension implementation.
- **`/Classes`**: Contains core data models and helper classes.
  - [`SettingsManager.cs`](./Classes/SettingsManager.cs) - Manages user-configurable settings.
  - `VisualStudioCodeInstance.cs`, `VisualStudioCodeWorkspace.cs` - Core data models.
- **`/Commands`**: Contains command implementations that are executed by the user.
    - `OpenVisualStudioCodeCommand.cs` - The primary command for launching a selected VS Code workspace.
    - `OpenSolutionCommand.cs` - The primary command for launching a selected Visual Studio solution.
    - `CopyPathCommand.cs` - Copies the workspace or solution path to the clipboard.
    - `RemoveWorkspaceCommand.cs` - Removes a workspace from the recently opened list.
    - `PinWorkspaceCommand.cs` - Pins or unpins a workspace.
    - `RefreshWorkspacesCommand.cs` - Manually refreshes the workspace list.
    - `OpenInTerminalCommand.cs` - Opens the workspace location in a terminal.
    - `ResetFrequencyCommand.cs` - Resets the usage frequency of a workspace.
- **`/Components`**: Contains components for window management, adapted from the WindowWalker extension.
    - `Window.cs` - Represents a single open window.
    - `WindowProcess.cs` - Manages process information for a window.
    - `OpenWindows.cs` - Manages the collection of open windows.
- **`/Helpers`**: Contains helper classes, including `NativeMethods.cs` for P/Invoke signatures.
- **`/Interfaces`**: Defines the contracts for services.
  - `IVisualStudioCodeService.cs` - The contract for the main service.
- **`/Pages`**: Contains UI components.
  - [`VisualStudioCodePage.cs`](./Pages/VisualStudioCodePage.cs) - A dynamic list page that displays discovered workspaces and solutions.
- **`/Services`**: Contains the primary service implementations.
  - [`WorkspaceLauncherForVSCodeCommandsProvider.cs`](./Services/WorkspaceLauncherForVSCodeCommandsProvider.cs) - The entry point for providing commands to the Command Palette.
  - [`VisualStudioCodeService.cs`](./Services/VisualStudioCodeService.cs) - Acts as a facade, orchestrating calls to more specialized provider classes.
  - [`VisualStudioCodeInstanceProvider.cs`](./Services/VisualStudioCodeInstanceProvider.cs) - Discovers all installed instances of Visual Studio Code.
  - [`VisualStudioCodeWorkspaceProvider.cs`](./Services/VisualStudioCodeWorkspaceProvider.cs) - Orchestrates the process of finding recent VS Code workspaces.
  - [`VisualStudioProvider.cs`](./Services/VisualStudioProvider.cs) - Orchestrates the process of finding recent Visual Studio solutions.
  - [`PinService.cs`](./Services/PinService.cs) - Manages the pinned state of workspaces.
  - [`VSWorkspaceWatcherService.cs`](./Services/VSWorkspaceWatcherService.cs) - Monitors Visual Studio settings files for changes to trigger auto-refresh.
  - [`VSCodeWorkspaceWatcherService.cs`](./Services/VSCodeWorkspaceWatcherService.cs) - Monitors Visual Studio Code storage for changes to trigger auto-refresh.
  - **`/Services/VisualStudio`**: Contains the integrated source code for discovering Visual Studio instances and their recent items.
- **`/Workspaces`**: Contains workspace-related logic.
    - **`/Models`**: C# models that map to the JSON structures of Visual Studio Code's workspace storage.
    - **`/Readers`**: Specialized classes for reading and parsing workspace data.
        - [`VscdbWorkspaceReader.cs`](./Workspaces/Readers/VscdbWorkspaceReader.cs) - Reads workspace data from the `state.vscdb` SQLite database.
        - [`StorageJsonWorkspaceReader.cs`](./Workspaces/Readers/StorageJsonWorkspaceReader.cs) - Reads workspace data from the `storage.json` file.

## Core Components

### WindowWalker Integration

The extension integrates logic from the **WindowWalker** extension to provide window-switching functionality. When a user attempts to open a Visual Studio solution, the extension first checks if that solution is already open in an existing Visual Studio instance.

- The `OpenWindows` class enumerates all open windows on the system.
- The `Window` and `WindowProcess` classes gather information about each window, including its title and process name.
- The `OpenSolutionCommand` checks the titles of open `devenv.exe` processes to see if the solution is already open. If a match is found, the extension switches to that window instead of launching a new process.

### Services and Providers

*   **`IVisualStudioCodeService` / `VisualStudioCodeService`**: This service acts as the primary entry point for interacting with local Visual Studio and Visual Studio Code data. It delegates the complex tasks of instance and workspace/solution discovery to specialized provider classes.
*   **`VisualStudioCodeInstanceProvider`**: A static provider class responsible for discovering all installed instances of Visual Studio Code (Stable, Insiders, User, System) by scanning known locations and the system's PATH environment variable.
*   **`VisualStudioCodeWorkspaceProvider`**: This static provider orchestrates the process of finding recently opened VS Code workspaces. It doesn't perform the reading itself but delegates the task to specialized reader classes.
*   **`VisualStudioProvider`**: This static provider orchestrates the process of finding recently opened Visual Studio solutions. It uses the integrated `VisualStudioService` to discover Visual Studio instances and their recent items.

### Watcher Services

*   **`VSWorkspaceWatcherService` & `VSCodeWorkspaceWatcherService`**: These services provide real-time updates by monitoring the underlying configuration files (`ApplicationPrivateSettings.xml` for VS, `storage.json` / `state.vscdb` for VS Code). When a change is detected (e.g., a new solution is opened), the extension automatically refreshes the list.

### Pin Service

*   **`PinService`**: Manages the pinning functionality, allowing users to keep frequently used workspaces at the top of the list. It persists the pinned state across sessions.

### Visual Studio Integration

The extension now includes the core logic from the `PowerToys-Run-VisualStudio` project to discover Visual Studio installations and their recent solutions. This is accomplished by:
1.  Using `vswhere.exe` to find all Visual Studio installations on the system.
2.  Parsing the `ApplicationPrivateSettings.xml` file for each installation to find the location of the `CodeContainers.json` file.
3.  Reading and deserializing the `CodeContainers.json` file to get a list of recent solutions.

This integration provides a seamless experience, allowing users to access both Visual Studio solutions and VS Code workspaces from a single interface.

**Asynchronous Initialization**:
To prevent UI freezing, the discovery of Visual Studio instances (which involves running `vswhere.exe`) and the scanning of their settings files are performed asynchronously in the background. The `InitInstancesAsync` method ensures that the heavy lifting is done without blocking the main thread, providing a responsive user experience even during startup.

### Workspace Readers and Performance

To ensure high performance and low memory usage, the extension uses the `System.Text.Json` source generator for deserializing workspace data. This avoids runtime reflection and minimizes allocations.

*   **Native AOT**: The application supports Native AOT (Ahead-of-Time) compilation, which significantly reduces startup time and memory footprint (introduced in version 1.9.0.0).
*   **Asynchronous Loading**: All heavy I/O operations, including reading workspace databases and running external processes, are fully asynchronous to maintain UI responsiveness.
*   **`VscdbWorkspaceReader` & `StorageJsonWorkspaceReader`**: These static reader classes are responsible for retrieving workspace information from Visual Studio Code's two primary data sources: the `state.vscdb` SQLite database and the `storage.json` file. Each reader is optimized to read its specific source and deserialize the data efficiently using source-generated models.

### Settings

*   **`SettingsManager`**: Manages all user-configurable settings for the extension. It loads settings from a JSON file and provides them to the rest of the application. It raises an event when settings are changed, allowing other components to react accordingly.

### UI and Commands

*   **`WorkspaceLauncherForVSCodeCommandsProvider`**: The main entry point for providing commands to the Command Palette. It initializes the required services and creates the top-level command items.
*   **`VisualStudioCodePage`**: A dynamic list page that displays the discovered workspaces and solutions. It receives the `IVisualStudioCodeService` to fetch data asynchronously and handles user interactions like searching and scrolling.
*   **`OpenVisualStudioCodeCommand` & `OpenSolutionCommand`**: The primary commands responsible for launching a selected VS Code workspace or Visual Studio solution.

#### Secondary Commands
Beyond the primary action of opening a workspace or solution, the extension provides secondary commands for managing the list:

*   **`CopyPathCommand`**: This command allows the user to copy the full file path of a workspace, solution, or folder directly to the clipboard.
*   **`RemoveWorkspaceCommand`**: This command provides the functionality to remove a workspace entry from the Visual Studio Code's list of recently opened items (not applicable to Visual Studio solutions).
*   **`PinWorkspaceCommand`**: (Un)pins a workspace or solution.
*   **`OpenInTerminalCommand`**: Opens the selected workspace's folder in the configured terminal.

This architecture ensures a clean separation of concerns, making the codebase easier to understand, extend, and debug.