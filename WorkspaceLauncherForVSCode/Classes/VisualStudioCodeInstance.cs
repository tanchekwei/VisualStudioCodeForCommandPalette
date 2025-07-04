// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Text.Json.Serialization;
using Microsoft.CommandPalette.Extensions.Toolkit;
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
    public static IconInfo Icon => Classes.Icon.VisualStudioCode;

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
        this.Name = name;
        this.ExecutablePath = executablePath;
        this.StoragePath = storagePath;
        this.InstallationType = installationType;
        this.VisualStudioCodeType = type;
    }
}
