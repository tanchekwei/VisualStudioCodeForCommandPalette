// Modifications Copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Net;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Helpers
{
    public static class WslPathHelper
    {
        public static bool TryGetWindowsPathFromWslUri(string wslUri, out string? windowsPath)
        {
            try
            {
                windowsPath = null;

                if (TryGetWslPath(wslUri, out var distro, out var wslPath))
                {
                    windowsPath = $"\\\\wsl$\\{distro}{wslPath.Replace('/', '\\')}";
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                windowsPath = null;
                return false;
            }
        }

        private static bool TryGetWslPath(string uri, out string distro, out string wslPath)
        {
            try
            {
                distro = string.Empty;
                wslPath = string.Empty;

                if (uri.StartsWith("vscode-remote://", StringComparison.OrdinalIgnoreCase))
                {
                    var pathPart = uri.Substring("vscode-remote://".Length);
                    var authorityEndIndex = pathPart.IndexOf('/');
                    if (authorityEndIndex <= 0) return false;

                    var encodedAuthority = pathPart.Substring(0, authorityEndIndex);
                    wslPath = pathPart.Substring(authorityEndIndex);
                    var authority = WebUtility.UrlDecode(encodedAuthority);

                    if (!authority.StartsWith("wsl+", StringComparison.Ordinal)) return false;

                    distro = authority.Substring("wsl+".Length);
                    return true;
                }
                else if (uri.StartsWith("file://wsl.localhost/", StringComparison.OrdinalIgnoreCase))
                {
                    var pathPart = uri.Substring("file://wsl.localhost/".Length);
                    var firstSlashIndex = pathPart.IndexOf('/');
                    if (firstSlashIndex == -1) return false;

                    distro = pathPart.Substring(0, firstSlashIndex);
                    wslPath = pathPart.Substring(firstSlashIndex);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                distro = string.Empty;
                wslPath = string.Empty;
                return false;
            }
        }
    }
}