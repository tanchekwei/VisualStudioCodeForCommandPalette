// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Helpers;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models;

namespace WorkspaceLauncherForVSCode;

/// <summary>
/// Represents a Visual Studio Code workspace.
/// </summary>
public class VisualStudioCodeWorkspace : IEquatable<VisualStudioCodeWorkspace>
{
    public string? Path { get; set; }
    public string? Name { get; set; }
    public VisualStudioCodeInstance? VSCodeInstance { get; set; }
    public VisualStudioInstance? VSInstance { get; set; }
    public string? WindowsPath { get; set; }
    public WorkspaceType WorkspaceType { get; set; }
    public VisualStudioCodeWorkspaceSource Source { get; set; }
    public List<string> SourcePath { get; set; } = new();
    public string WorkspaceName { get; set; } = "";
    public VisualStudioCodeRemoteUri? VisualStudioCodeRemoteUri { get; set; }
    public string WorkspaceTypeString { get; set; } = "";
    public int Frequency { get; set; }
    public DateTime LastAccessed { get; set; }
    public DateTime VSLastAccessed { get; set; }
    public DateTime? PinDateTime { get; set; }

    public VisualStudioCodeWorkspace() { }
    /// <summary>
    /// Initializes a new instance of the <see cref="VisualStudioCodeWorkspace"/> class.
    /// </summary>
    /// <param name="instance">The Visual Studio Code instance associated with the workspace.</param>
    /// <param name="path">The path to the workspace.</param>
    internal VisualStudioCodeWorkspace(VisualStudioCodeInstance instance, string path, WorkspaceType visualStudioCodeWorkspaceType, VisualStudioCodeWorkspaceSource source, string sourcePath)
    {
        Path = path;
        if (VisualStudioCodeRemoteUri.IsVisualStudioCodeRemoteUri(path))
        {
            VisualStudioCodeRemoteUri = new(path);
            WindowsPath = VisualStudioCodeRemoteUri?.GetSubtitle();
        }
        else if (FileUriParser.TryParse(path, out var windowsPath))
        {
            WindowsPath = windowsPath;
        }
        VSCodeInstance = instance;
        WorkspaceType = visualStudioCodeWorkspaceType;
        Source = source;
        SourcePath.Add(sourcePath);

        SetName();
        SetWorkspaceType();
        SetVSCodeMetadata();
        Name = WorkspaceName;
    }

    /// <summary>
    /// Sets the name of the workspace.
    /// </summary>
    /// <returns>The name of the workspace.</returns>
    public void SetName()
    {
        if (Path == null) return;
        WorkspaceName = "";

        // split name by / and get last part
        var nameParts = Uri.UnescapeDataString(Path).Split('/');
        if (nameParts.Length == 0)
        {
            return;
        }

        WorkspaceName = nameParts[nameParts.Length - 1];

        if (WorkspaceType == WorkspaceType.Workspace)
        {
            // remove .code-workspace
            WorkspaceName = WorkspaceName.Replace(".code-workspace", "", StringComparison.OrdinalIgnoreCase);

            // if the workspace name is "workspace", use the folder name instead
            if (WorkspaceName.Equals("workspace", StringComparison.OrdinalIgnoreCase) && nameParts.Length >= 2)
            {
                WorkspaceName = nameParts[nameParts.Length - 2];
            }
        }
    }

    /// <summary>
    /// Sets the workspace type (e.g., Workspace, Folder).
    /// </summary>
    /// <returns>The type of the workspace as a string.</returns>
    public void SetWorkspaceType()
    {
        switch (WorkspaceType)
        {
            case WorkspaceType.Workspace:
                WorkspaceTypeString = "Workspace";
                break;
            case WorkspaceType.Folder:
                WorkspaceTypeString = "Folder";
                break;
            case WorkspaceType.Solution:
                WorkspaceTypeString = "Solution";
                break;
            default:
                WorkspaceTypeString = "Unknown Type";
                break;
        }
    }

    /// <summary>
    /// Gets the details of the workspace.
    /// </summary>
    /// <returns>An array of details elements containing information about the workspace.</returns>
    public void SetVSCodeMetadata()
    {
        if (Path == null) return;
        var typeTags = new List<Tag>() { new Tag(WorkspaceTypeString) };
        if (VisualStudioCodeRemoteUri != null)
        {
            typeTags.Add(new Tag(VisualStudioCodeRemoteUri.Type.ToDisplayName()));
        }
        else if (VisualStudioCodeRemoteUri?.TypeStr != null)
        {
            typeTags.Add(new Tag(VisualStudioCodeRemoteUri.TypeStr));
        }
    }

    /// <summary>
    /// Gets the details of the workspace.
    /// </summary>
    /// <returns>An array of details elements containing information about the workspace.</returns>
    public static void SetVSMetadata()
    {
    }

    public string GetWorkspaceName()
    {
        string workspaceName;
        if (VisualStudioCodeRemoteUri is null)
        {
            return WorkspaceName;
        }
        switch (VisualStudioCodeRemoteUri.Type)
        {
            case VisualStudioCodeRemoteType.DevContainer:
                if (string.IsNullOrEmpty(WindowsPath) || VisualStudioCodeRemoteUri.IsVisualStudioCodeRemoteUri(WindowsPath))
                {
                    workspaceName = WorkspaceName;
                }
                else
                {
                    workspaceName = System.IO.Path.GetFileName(WindowsPath) ?? WorkspaceName;
                }
                break;
            default:
                workspaceName = WorkspaceName;
                break;
        }
        return workspaceName;
    }

    public bool Equals(VisualStudioCodeWorkspace? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Path == other.Path;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as VisualStudioCodeWorkspace);
    }

    public override int GetHashCode()
    {
        return Path?.GetHashCode() ?? 0;
    }
}
