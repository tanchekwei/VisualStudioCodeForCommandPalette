// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Text.Json;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Pages;

public sealed partial class DetailPage : ContentPage
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly VisualStudioCodeWorkspaceSerializerContext _serializerContext =
        new(_jsonOptions);

    private const string TitleText = "Detail";
    private const string NameText = "Detail";
    private const string IdText = "Detail";
    private const string MarkdownPrefix = "```json\n";
    private const string MarkdownSuffix = "\n```";

    readonly VisualStudioCodeWorkspace Workspace;

    public DetailPage(VisualStudioCodeWorkspace workspace)
    {
        Title = TitleText;
        Name = NameText;
        Id = IdText;
        Icon = Classes.Icon.Bug;
        Workspace = workspace;
    }

    public override IContent[] GetContent()
    {
        var json = JsonSerializer.Serialize(Workspace, _serializerContext.VisualStudioCodeWorkspace);
        var markdown = $"{MarkdownPrefix}{json}{MarkdownSuffix}";
        return [new MarkdownContent(markdown)];
    }
}
