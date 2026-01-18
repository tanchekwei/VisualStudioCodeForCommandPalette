// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Text.Json.Serialization;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode;

/// <summary>
/// Represents an instance of Visual Studio Code.
/// </summary>
public class VisualStudioCodeInstance
{
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public VisualStudioCodeInstallationType InstallationType { get; set; }
    public VisualStudioCodeType VisualStudioCodeType { get; set; }
    [JsonIgnore]
    public IconInfo CachedIcon { get; private set; }
    [JsonIgnore]
    public string DisplayName { get; private set; }

    public VisualStudioCodeInstance() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="VisualStudioCodeInstance"/> class.
    /// </summary>
    /// <param name="name">The name of the Visual Studio Code instance.</param>
    /// <param name="executablePath">The path to the executable file.</param>
    /// <param name="storagePath">The path to the storage file.</param>
    /// <param name="installationType">The type of installation (system or user).</param>
    /// <param name="type">The type of Visual Studio Code (default or insider).</param>
    internal VisualStudioCodeInstance(string name, string executablePath, string storagePath, VisualStudioCodeInstallationType installationType, VisualStudioCodeType type)
    {
        try
        {
            this.Name = name;
            this.ExecutablePath = executablePath;
            this.StoragePath = storagePath;
            this.InstallationType = installationType;
            this.VisualStudioCodeType = type;

            this.DisplayName = name?.Replace("VS Code", "Visual Studio Code") ?? string.Empty;
            this.CachedIcon = GetIconForType(type);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            this.DisplayName = name ?? string.Empty;
            this.CachedIcon = Classes.Icon.VisualStudioCode;
        }
    }

    /// <summary>
    /// Gets the icon associated with the Visual Studio Code instance.
    /// </summary>
    /// <returns>An icon representing the Visual Studio Code instance.</returns>
    private static IconInfo GetIconForType(VisualStudioCodeType type)
    {
        try
        {
            return type switch
            {
                VisualStudioCodeType.Insider => Classes.Icon.VisualStudioCodeInsiders,
                VisualStudioCodeType.Cursor => Classes.Icon.Cursor,
                VisualStudioCodeType.Antigravity => Classes.Icon.Antigravity,
                VisualStudioCodeType.Windsurf => Classes.Icon.Windsurf,
                _ => Classes.Icon.VisualStudioCode
            };
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return Classes.Icon.VisualStudioCode;
        }
    }
}
