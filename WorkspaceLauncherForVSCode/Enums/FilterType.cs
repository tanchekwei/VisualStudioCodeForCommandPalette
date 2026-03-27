// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;

namespace WorkspaceLauncherForVSCode.Enums
{
    [Flags]
    public enum FilterType
    {
        All,
        Vscode,
        VisualStudio2026,
        VisualStudio,
        VscodeInsider,
        Cursor,
        Antigravity,
        Windsurf,
        Folder,
        Workspace,
        RemoteWsl,
        RemoteDevContainer,
        RemoteCodespaces,
        RemoteAttachedContainer,
        RemoteSSHRemote
    }
}