// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Classes;

public static class Constant
{
#if DEBUG
    public const string AppName = "WorkspaceLauncherForVSCodeDev";
#else
    public const string AppName = "WorkspaceLauncherForVSCode";
#endif
#if DEBUG
    public const string VisualStudioCodeDisplayName = "Visual Studio / Code (Dev)";
#else
    public const string VisualStudioCodeDisplayName = "Visual Studio / Code";
#endif
    public const string VscodeRemoteScheme = "vscode-remote://";
    public const string VisualStudio2026Version = "18";
    public static readonly string AssemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
    public static readonly string SettingsFolderPath = Utilities.BaseSettingsPath(AppName);

    public static class FallbackIndex
    {
        public const int OpenInVisualStudioCode = 0;
        public const int StartOfDynamicFallback = 1;
    }
}
