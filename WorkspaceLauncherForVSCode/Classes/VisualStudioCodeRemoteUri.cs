using System;
using System.Net;
using System.Text;
using System.Text.Json;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Classes;

public class VisualStudioCodeRemoteUri
{
    public const string Scheme = Constant.VscodeRemoteScheme;
    public string Uri { get; }
    public string TypeStr { get; }
    public VisualStudioCodeRemoteType? Type { get; }
    public string InfoRaw { get; }
    public string InfoDecoded { get; }
    public JsonElement? InfoJson { get; }
    public string Path { get; }

    public VisualStudioCodeRemoteUri(string uri)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("URI cannot be null or empty.", nameof(uri));

            if (!uri.StartsWith(Constant.VscodeRemoteScheme, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("URI must start with 'vscode-remote://'");

            Uri = uri;

            // Remove scheme
            var withoutScheme = uri.Substring(Constant.VscodeRemoteScheme.Length);

            // Detect percent-encoded '+' (e.g., Codespaces)
            var firstSlash = withoutScheme.IndexOf('/');
            var beforePath = firstSlash == -1 ? withoutScheme : withoutScheme.Substring(0, firstSlash);
            var path = firstSlash == -1 ? "/" : withoutScheme.Substring(firstSlash);

            // Handle encoded `+` (for codespaces)
            string remoteType = string.Empty, remoteInfoRaw = string.Empty;

            if (beforePath.Contains('+'))
            {
                var parts = beforePath.Split('+', 2);
                remoteType = parts[0];
                remoteInfoRaw = parts[1];
            }
            else if (beforePath.Contains("%2B", StringComparison.OrdinalIgnoreCase))
            {
                var decoded = WebUtility.UrlDecode(beforePath); // decode first
                var parts = decoded.Split('+', 2);
                remoteType = parts[0];
                remoteInfoRaw = parts[1];
            }
            else
            {
                throw new ArgumentException("Malformed vscode-remote URI: '+' not found.");
            }

            TypeStr = remoteType;
            if (VisualStudioCodeRemoteTypeHelper.TryParse(remoteType, out var type))
            {
                Type = type;
            }
            InfoRaw = remoteInfoRaw;
            Path = path;

            // Try to detect and decode hex-encoded JSON (must be even length and valid hex)
            InfoDecoded = TryDecodeHexJson(remoteInfoRaw, out var jsonElement)
                ? jsonElement.ToString()
                : InfoRaw;

            InfoJson = jsonElement.ValueKind != JsonValueKind.Undefined ? jsonElement : null;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            Uri = string.Empty;
            TypeStr = string.Empty;
            InfoRaw = string.Empty;
            InfoDecoded = string.Empty;
            Path = string.Empty;
            throw;
        }
    }

    private static bool TryDecodeHexJson(string hex, out JsonElement element)
    {
        element = default;

        try
        {
            // Rough check: valid hex, even length
            if (hex.Length % 2 != 0 || !IsHexString(hex))
                return false;

            var bytes = Convert.FromHexString(hex);
            var json = Encoding.UTF8.GetString(bytes);

            var doc = JsonDocument.Parse(json);
            element = doc.RootElement.Clone();
            return true;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return false;
        }
    }

    private static bool IsHexString(string s)
    {
        try
        {
            foreach (char c in s)
            {
                if (!System.Uri.IsHexDigit(c))
                    return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return false;
        }
    }

    public override string ToString()
    {
        try
        {
            return $"Type: {Type}\nInfo: {InfoDecoded}\nPath: {Path}";
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return string.Empty;
        }
    }

    public static bool IsVisualStudioCodeRemoteUri(string uri)
    {
        try
        {
            return uri.StartsWith(Constant.VscodeRemoteScheme, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return false;
        }
    }

    public string GetSubtitle()
    {
        try
        {
            string? subtitle = null;
            switch (Type)
            {
                case VisualStudioCodeRemoteType.Codespaces:
                    subtitle = GetCodespacesUrl(InfoRaw);
                    break;
                case VisualStudioCodeRemoteType.DevContainer:
                    subtitle = GetDevContainerSubtitle();
                    break;
                case VisualStudioCodeRemoteType.WSL:
                    WslPathHelper.TryGetWindowsPathFromWslUri(Uri, out var windowsPath);
                    subtitle = windowsPath;
                    break;
                default:
                    break;
            }
            return string.IsNullOrWhiteSpace(subtitle) ? GetDecodedUri() : subtitle;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return string.Empty;
        }
    }

    public static string GetCodespacesUrl(string subdomain)
    {
        try
        {
            return $"{System.Uri.UriSchemeHttps}://{subdomain}.github.dev/";
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return string.Empty;
        }
    }

    public string? GetDevContainerSubtitle()
    {
        try
        {
            if (InfoJson.HasValue && InfoJson.Value.TryGetProperty("hostPath", out JsonElement hostPathElement))
            {
                return FileUriParser.CapitalizeDriveLetter(hostPathElement.GetString() ?? string.Empty);
            }
            return null;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return null;
        }
    }

    public string GetDecodedUri()
    {
        try
        {
            return $"{Scheme}{TypeStr}+{InfoDecoded}{Path}";
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return string.Empty;
        }
    }
}
