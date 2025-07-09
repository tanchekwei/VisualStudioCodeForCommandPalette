// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

namespace WorkspaceLauncherForVSCode.Classes;

public static class Constant
{
#if DEBUG
    public const string AppName = "WorkspaceLauncherForVSCodeDev";
#else
    public const string AppName = "WorkspaceLauncherForVSCode";
#endif
    public const string VscodeRemoteScheme = "vscode-remote://";
}
