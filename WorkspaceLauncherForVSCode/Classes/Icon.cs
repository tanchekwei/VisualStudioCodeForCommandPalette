// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Classes;

public static class Icon
{
    public static readonly IconInfo Extension = IconHelpers.FromRelativePath(@"Assets\icon.svg");
    public static readonly IconInfo VisualStudio = IconHelpers.FromRelativePath(@"Assets\VisualStudioIcon.svg");
    public static readonly IconInfo VisualStudioCode = IconHelpers.FromRelativePath("Assets\\VisualStudioCodeIcon.svg");
    public static readonly IconInfo VisualStudioAndVisualStudioCode = IconHelpers.FromRelativePath("Assets\\VisualStudioAndVisualStudioCodeIcon.svg");
    public static readonly IconInfo FileExplorer = IconHelpers.FromRelativePath("Assets\\FileExplorer.svg");
    public static readonly IconInfo GitHub = IconHelpers.FromRelativePath("Assets\\github-mark-white.svg");
    public static readonly IconInfo Pinned = new IconInfo("\ue718");
    public static readonly IconInfo Unpinned = new IconInfo("\ue77a");
    public static readonly IconInfo Web = new("\uE774");
    public static readonly IconInfo Help = new IconInfo("\uE897");
    public static readonly IconInfo Info = new IconInfo("\uE946");
    public static readonly IconInfo Setting = new IconInfo("\uE713");
    public static readonly IconInfo Refresh = new IconInfo("\xE72C");
    public static readonly IconInfo Copy = new IconInfo("\uE8c8");
}
