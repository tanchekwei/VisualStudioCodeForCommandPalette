// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Helpers
{
    public static class FileUriParser
    {
        public static bool TryParse(
            string inputPath,
            [NotNullWhen(true)] out string? windowsPath)
        {
            try
            {
                windowsPath = null;
                if (string.IsNullOrWhiteSpace(inputPath))
                {
                    return false;
                }

                return TryParseFileUri(inputPath, out windowsPath);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                windowsPath = null;
                return false;
            }
        }

        private static bool TryParseFileUri(string path, [NotNullWhen(true)] out string? windowsPath)
        {
            try
            {
                path = path.Replace("%3A", ":", StringComparison.OrdinalIgnoreCase);

                if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile)
                {
                    windowsPath = CapitalizeDriveLetter(uri.LocalPath.TrimStart('/'));
                    return true;
                }

                windowsPath = path;
                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                windowsPath = null;
                return false;
            }
        }

        public static string? HexToJson(string hex)
        {
            try
            {
                var bytes = Convert.FromHexString(hex);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return null;
            }
        }

        public static string CapitalizeDriveLetter(string path)
        {
            try
            {
                if (path.Length >= 2 && path[1] == ':' && char.IsLower(path[0]))
                {
                    return string.Create(path.Length, path, static (span, state) =>
                    {
                        state.AsSpan().CopyTo(span);
                        span[0] = char.ToUpperInvariant(state[0]);
                    });
                }
                return path;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return path;
            }
        }
    }
}