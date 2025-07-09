// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using WorkspaceLauncherForVSCode;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Helpers
{
    public static class FileUriParser
    {
        public static bool TryParse(
            string inputPath,
            [NotNullWhen(true)] out string? windowsPath)
        {
            windowsPath = null;
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                return false;
            }

            // if (inputPath.StartsWith(Constant.VscodeRemoteScheme, StringComparison.OrdinalIgnoreCase))
            // {
            //     return TryParseRemoteUri(inputPath, out windowsPath);
            // }

            return TryParseFileUri(inputPath, out windowsPath);
        }

        private static bool TryParseFileUri(string path, [NotNullWhen(true)] out string? windowsPath)
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

        // private static bool TryParseRemoteUri(
        //     string path,
        //     [NotNullWhen(true)] out string? windowsPath)
        // {
        //     windowsPath = null;
        //     var decodedPath = System.Net.WebUtility.UrlDecode(path);
        //     int start = decodedPath.IndexOf(Constant.VscodeRemoteScheme, StringComparison.OrdinalIgnoreCase) + Constant.VscodeRemoteScheme.Length;
        //     int end = decodedPath.IndexOf('+', start);
        //     if (end == -1)
        //     {
        //         return false;
        //     }

        //     var typeString = decodedPath.Substring(start, end - start);
        //     if (VisualStudioCodeRemoteHelper.TryParse(typeString.Trim(), out var parsedRemoteType))
        //     {
        //         remoteType = parsedRemoteType;
        //     }
        //     else
        //     {
        //         string spaced = typeString.Replace('-', ' ');
        //         TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
        //         remoteTypeStr = textInfo.ToTitleCase(spaced.ToLowerInvariant());
        //     }

        //     remoteTypeData = decodedPath.Substring(end + 1);
        //     int slashIndex = remoteTypeData.IndexOf('/');
        //     remoteTypeDataFirstSegment = slashIndex >= 0 ? remoteTypeData.Substring(0, slashIndex) : remoteTypeData;
        //     var remaining = slashIndex >= 0 ? remoteTypeData.Substring(slashIndex) : string.Empty;

        //     if (remoteType == VisualStudioCodeRemoteType.Codespaces)
        //     {
        //         windowsPath = path;
        //     }
        //     else
        //     {
        //         windowsPath = $"{Constant.VscodeRemoteScheme}{typeString}+{remoteTypeDataFirstSegment}{remaining}";
        //     }

        //     return true;
        // }

        public static string? HexToJson(string hex)
        {
            try
            {
                var bytes = new byte[hex.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string CapitalizeDriveLetter(string path)
        {
            if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
            {
                return char.ToUpperInvariant(path[0]) + path.Substring(1);
            }
            return path;
        }
    }
}
