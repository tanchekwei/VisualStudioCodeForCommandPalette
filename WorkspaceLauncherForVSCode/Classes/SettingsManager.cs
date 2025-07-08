// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Properties;

namespace WorkspaceLauncherForVSCode;

public class SettingsManager : JsonSettingsManager
{
    private static readonly string _namespace = "vscode";

    private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";

    private static readonly List<ChoiceSetSetting.Choice> _preferredEditionChoices =
    [
        new ChoiceSetSetting.Choice(Resource.setting_preferredEdition_option_default_label, "Default"),
        new ChoiceSetSetting.Choice(Resource.setting_preferredEdition_option_insider_label, "Insider"),
    ];

    private static readonly List<ChoiceSetSetting.Choice> _commandResultChoices =
    [
        new ChoiceSetSetting.Choice(Resource.setting_commandResult_option_dismiss_label, nameof(CommandResultType.Dismiss)),
        new ChoiceSetSetting.Choice(Resource.setting_commandResult_option_goback_label, nameof(CommandResultType.GoBack)),
        new ChoiceSetSetting.Choice(Resource.setting_commandResult_option_keepopen_label, nameof(CommandResultType.KeepOpen)),
    ];

    private static readonly List<ChoiceSetSetting.Choice> _searchByChoices =
    [
        new ChoiceSetSetting.Choice("Both", nameof(SearchBy.Both)),
        new ChoiceSetSetting.Choice("Title", nameof(SearchBy.Title)),
        new ChoiceSetSetting.Choice("Path", nameof(SearchBy.Path)),
    ];

    private static readonly List<ChoiceSetSetting.Choice> _vscodeSecondaryCommandChoices =
    [
        new ChoiceSetSetting.Choice("Open in Explorer", nameof(SecondaryCommand.OpenInExplorer)),
        new ChoiceSetSetting.Choice("Run as Administrator", nameof(SecondaryCommand.OpenAsAdministrator)),
    ];

    private static readonly List<ChoiceSetSetting.Choice> _vsSecondaryCommandChoices =
    [
        new ChoiceSetSetting.Choice("Run as Administrator", nameof(SecondaryCommand.OpenAsAdministrator)),
        new ChoiceSetSetting.Choice("Open in Explorer", nameof(SecondaryCommand.OpenInExplorer)),
    ];

    private readonly ToggleSetting _enableLogging = new(
        Namespaced(nameof(EnableLogging)),
        "Enable Logging",
        "Enables diagnostic logging for troubleshooting.",
        false);

    private readonly ToggleSetting _showTypeTag = new(
        Namespaced(nameof(_showTypeTag)),
        Resource.setting_tagType_option_type_label,
        "Show the 'Type' tag (Workspace/Folder)",
        false);

    private readonly ToggleSetting _showTargetTag = new(
        Namespaced(nameof(_showTargetTag)),
        Resource.setting_tagType_option_target_label,
        "Show the 'Target' tag (Visual Studio Code/Insiders)",
        false);

    private readonly ToggleSetting _enableVisualStudio = new(
        Namespaced(nameof(_enableVisualStudio)),
        "Enable Visual Studio",
        "Enable Visual Studio installation",
        true);

    private readonly ToggleSetting _enableDefault = new(
        Namespaced(nameof(_enableDefault)),
        "Enable Visual Studio Code",
        "Enable the default Visual Studio Code installation",
        true);

    private readonly ToggleSetting _enableSystem = new(
        Namespaced(nameof(_enableSystem)),
        "Enable Visual Studio Code (System)",
        "Enable the system-wide Visual Studio Code installation",
        true);

    private readonly ToggleSetting _enableInsider = new(
        Namespaced(nameof(_enableInsider)),
        "Enable Visual Studio Code - Insiders",
        "Enable the Insiders Visual Studio Code installation",
        false);

    private readonly ToggleSetting _enableCustom = new(
        Namespaced(nameof(_enableCustom)),
        "Enable Visual Studio Code (Custom)",
        "Enable custom Visual Studio Code installations found in the PATH",
        false);

    private readonly ChoiceSetSetting _preferredEdition = new(
        Namespaced(nameof(PreferredEdition)),
        Resource.setting_preferredEdition_label,
        Resource.setting_preferredEdition_desc,
        _preferredEditionChoices);

    private readonly ChoiceSetSetting _commandResult = new(
        Namespaced(nameof(CommandResult)),
        Resource.setting_commandResult_label,
        Resource.setting_commandResult_desc,
        _commandResultChoices);

    private readonly ChoiceSetSetting _searchBy = new(
        Namespaced(nameof(SearchBy)),
        "Search By",
        "Search by path, title or both.",
        _searchByChoices);

    private readonly ChoiceSetSetting _vsSecondaryCommand = new(
        Namespaced(nameof(VSSecondaryCommand)),
        "Visual Studio Secondary Command",
        "Configure the secondary command for Visual Studio solutions.",
        _vsSecondaryCommandChoices);

    private readonly ChoiceSetSetting _vscodeSecondaryCommand = new(
        Namespaced(nameof(VSCodeSecondaryCommand)),
        "Visual Studio Code Secondary Command",
        "Configure the secondary command for Visual Studio Code workspaces.",
        _vscodeSecondaryCommandChoices);

    private readonly TextSetting _pageSize = new(
        Namespaced(nameof(PageSize)),
        Resource.setting_pageSize_label,
        Resource.setting_pageSize_desc,
        "8");

    public bool EnableLogging => _enableLogging.Value;
    public string PreferredEdition => _preferredEdition.Value ?? "Default";
    public bool EnableVisualStudio => _enableVisualStudio.Value;

    public TagType TagTypes
    {
        get
        {
            var tagType = TagType.None;
            if (_showTypeTag.Value)
            {
                tagType |= TagType.Type;
            }
            if (_showTargetTag.Value)
            {
                tagType |= TagType.Target;
            }
            return tagType;
        }
        set
        {
            _showTypeTag.Value = value.HasFlag(TagType.Type);
            _showTargetTag.Value = value.HasFlag(TagType.Target);
        }
    }

    public VisualStudioCodeEdition EnabledEditions
    {
        get
        {
            var editions = VisualStudioCodeEdition.None;
            if (_enableDefault.Value) editions |= VisualStudioCodeEdition.Default;
            if (_enableSystem.Value) editions |= VisualStudioCodeEdition.System;
            if (_enableInsider.Value) editions |= VisualStudioCodeEdition.Insider;
            if (_enableCustom.Value) editions |= VisualStudioCodeEdition.Custom;
            return editions;
        }
    }

    public CommandResultType CommandResult
    {
        get
        {
            if (Enum.TryParse<CommandResultType>(_commandResult.Value, out var result))
            {
                return result;
            }
            return CommandResultType.Dismiss;
        }
    }

    public SearchBy SearchBy
    {
        get
        {
            if (Enum.TryParse<SearchBy>(_searchBy.Value, out var result))
            {
                return result;
            }
            return SearchBy.Both;
        }
    }

    public SecondaryCommand VSSecondaryCommand
    {
        get
        {
            if (Enum.TryParse<SecondaryCommand>(_vsSecondaryCommand.Value, out var result))
            {
                return result;
            }
            return SecondaryCommand.OpenAsAdministrator;
        }
    }

    public SecondaryCommand VSCodeSecondaryCommand
    {
        get
        {
            if (Enum.TryParse<SecondaryCommand>(_vscodeSecondaryCommand.Value, out var result))
            {
                return result;
            }
            return SecondaryCommand.OpenInExplorer;
        }
    }

    public int PageSize
    {
        get
        {
            if (int.TryParse(_pageSize.Value, out int size) && size > 0)
            {
                return size;
            }
            return 8;
        }
    }

    internal static string SettingsJsonPath()
    {
        var directory = Utilities.BaseSettingsPath(Constant.AppName);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "settings.json");
    }

    public SettingsManager()
    {
        FilePath = SettingsJsonPath();

        Settings.Add(_showTypeTag);
        Settings.Add(_showTargetTag);
        Settings.Add(_enableVisualStudio);
        Settings.Add(_enableDefault);
        Settings.Add(_enableSystem);
        Settings.Add(_enableInsider);
        Settings.Add(_enableCustom);
        Settings.Add(_preferredEdition);
        Settings.Add(_commandResult);
        Settings.Add(_pageSize);
        Settings.Add(_searchBy);
        Settings.Add(_vsSecondaryCommand);
        Settings.Add(_vscodeSecondaryCommand);
        Settings.Add(_enableLogging);

        // Load settings from file upon initialization
        LoadSettings();

        Settings.SettingsChanged += (s, a) =>
        {
            SaveSettings();
        };
    }
}
