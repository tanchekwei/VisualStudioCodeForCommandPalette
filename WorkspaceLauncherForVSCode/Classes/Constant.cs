// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

namespace WorkspaceLauncherForVSCode.Classes;

public static class Constant
{
#if DEBUG
  public static readonly string AppName = "WorkspaceLauncherForVSCodeDev";
#else
  public static readonly string AppName = "WorkspaceLauncherForVSCode";
#endif
  public static readonly string VscodeRemoteScheme = "vscode-remote://";
}
