// Modifications Copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Helpers;

public static class VisualStudioCodeRemoteTypeHelper
{
  private static readonly Dictionary<string, VisualStudioCodeRemoteType> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            { "wsl", VisualStudioCodeRemoteType.WSL },
            { "dev-container", VisualStudioCodeRemoteType.DevContainer },
            { "codespaces", VisualStudioCodeRemoteType.Codespaces },
            { "attached-container", VisualStudioCodeRemoteType.AttachedContainer },
            { "ssh-remote", VisualStudioCodeRemoteType.SSHRemote }
        };

  public static bool TryParse(string input, out VisualStudioCodeRemoteType env)
      => _map.TryGetValue(input, out env);

  public static string ToDisplayName(this VisualStudioCodeRemoteType? input)
  {
    if (input == null)
    {
      return string.Empty;
    }
    return input switch
    {
      VisualStudioCodeRemoteType.WSL => "WSL",
      VisualStudioCodeRemoteType.DevContainer => "Dev Container",
      VisualStudioCodeRemoteType.Codespaces => "Codespaces",
      VisualStudioCodeRemoteType.AttachedContainer => "Attached Container",
      VisualStudioCodeRemoteType.SSHRemote => "SSH Remote",
      _ => input.ToString()
    };
  }
}
