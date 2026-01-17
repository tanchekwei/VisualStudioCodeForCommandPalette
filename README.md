# Visual Studio / Code for Command Palette

## Overview

This project provides a command palette extension for opening Visual Studio solutions and Visual Studio Code workspaces from a single, unified interface.

![Visual Studio / Code for Command Palette](./Assets/screenshot1.png)

## Features
- **Optimized for Performance**: Designed to remain fast and responsive, with a configurable page size. By default, it displays 8 items per page to ensure smooth interaction even with large project histories.
- **Unified Launcher**: Launch Visual Studio solutions, Visual Studio Code, Cursor, and Google Antigravity workspaces from a single, convenient interface.
- **Window-Switching**: If a Visual Studio solution is already open, the extension will switch to the existing window instead of opening a new instance.
- **Secondary Actions**: Access additional commands for each entry:
    - **Copy Path**: Copies the full file path of the solution, workspace, or folder to the clipboard.
    - **Pin to List / Unpin from List**: Pins or unpins a workspace to keep it at the top of the list.
    - **Open in Explorer**: Opens the solution, workspace, or folder location in the default file explorer.
    - **Open in Terminal**: Opens the solution, workspace, or folder location in your preferred terminal (Command Prompt or PowerShell).
    - **Run as Administrator**: Open the solution, workspace, or folder with administrative privileges.
    - **Refresh Workspaces**: Manually reloads the list of solutions and workspaces to reflect any recent changes.

## Installation

> [!NOTE]  
> Because the application is first signed by the Microsoft Store, updates will take a few days to be available via WinGet or in the Command Palette.

### Windows Store

<a href="https://apps.microsoft.com/detail/9mvlfk6tr4d4?mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20light.svg" width="300"/>
</a>

### Via Command Palette

1. Open Command Palette
2. Select "Visual Studio / Code for Command Palette"

### Via Winget

1. Open Command Prompt or PowerShell
2. Run the following command:
   ```bash
   winget install 15722UsefulApp.WorkspaceLauncherForVSCode
   ```

### Manual Installation

1. Make sure you use the latest version of PowerToys.
2. Install the application by double-clicking the `.msix` file.

## Settings

* **General**

  * **Show Details Panel**: Toggles the visibility of the details panel on the right.
  * **Page Size**: Sets the number of items to display per page. (Default: `8`)
  * **Enable Logging**: Enables diagnostic logging for troubleshooting purposes.

* **Search & Appearance**
  * **Search By**: Determines which properties to use when searching (`Title`, `Path`, or `Both`).
  * **Sort By**: Determines the sorting order of workspaces.
      * `Last Used, Usage Count`: The default sort order.
      * `Last Used`: Sorts by most recently used.
      * `Usage Count`: Sorts by most frequently used.
      * `Alphabetical by Title`: Sorts alphabetically by title.
  * **Tags**: Configure which informational tags are displayed for each item.
    * **Show 'Type' tag**: Displays whether the item is a `Workspace` or `Folder`.
    * **Show 'Target' tag**: Displays the target application, like `Visual Studio Code` or `Insiders`.

* **Commands**
  * **Terminal Type**: Choose the terminal to be opened with the "Open in Terminal" command.
      * `PowerShell`
      * `Command Prompt`

* **Visual Studio**
  * **Enable Visual Studio**: Enables searching for Visual Studio solutions.
  * **Secondary Command**: Configures the secondary action for Visual Studio items (`Open in Explorer` or `Run as Administrator`).

* **Visual Studio Code**
  * **Enabled Installations**: Choose which VS Code installations to scan for workspaces.
    * **Enable Visual Studio Code**: The standard user-specific installation.
    * **Enable Visual Studio Code (System)**: The system-wide installation.
    * **Enable Visual Studio Code - Insiders**: The Insiders edition.
    * **Enable Visual Studio Code (Custom)**: Custom installations found in the system's `PATH`.
    * **Enable Cursor**: The Cursor AI code editor.
    * **Enable Google Antigravity**: The Google Antigravity editor.
    * **Configurable Paths**: You can specify custom installation paths for Cursor and Antigravity if they are not in the default locations.
  * **Secondary Command**: Configures the secondary action for Visual Studio Code items.

    * `Open in Explorer`: Opens the item's location in File Explorer.
    * `Run as Administrator`: Launches the item with administrative privileges.

## How It Works

This extension discovers installations of Visual Studio, Visual Studio Code, Cursor, and Google Antigravity on your system.
- For **Visual Studio Code**, **Cursor**, and **Antigravity**, it reads the workspace history from their respective internal storage files (`state.vscdb` and `storage.json`).
- For **Visual Studio**, it uses `vswhere.exe` to find installations and then reads their configuration files to discover recent solutions.
- The extension also includes logic from the **WindowWalker** extension to detect if a solution is already open. If so, it switches to the existing Visual Studio window instead of creating a new one.

The results are then combined into a single, unified list for easy access.

For more detailed technical information about the project's architecture and components, please see the [Project Guide](./GUIDE.md).

## Changelog

### 1.22.0.0
- Add support for Cursor and Google Antigravity editors
- Allow custom installation paths for Cursor and Antigravity

### 1.21.0.0
- Add option to open Visual Studio solutions in Visual Studio Code
- Add help command into refresh command context

### 1.9.0.0
- Native AOT compilation and trimming
- fix: WSL and Remote VS Code

## Contributing

Contributions are welcome! If you have suggestions for improvements or new features, please open an issue or submit a pull request.
