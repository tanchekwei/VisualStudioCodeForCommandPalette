// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;

namespace WorkspaceLauncherForVSCode.Enums
{
    [Flags]
    public enum VisualStudioCodeEdition
    {
        None = 0,
        Default = 1,
        System = 2,
        Insider = 4,
        Custom = 8,
        Cursor = 16,
        Antigravity = 32,
        Windsurf = 64
    }
}