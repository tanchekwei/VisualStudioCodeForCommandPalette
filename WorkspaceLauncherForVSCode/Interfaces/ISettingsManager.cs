using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Interfaces
{
    public interface ISettingsManager
    {
        bool ShowDetails { get; }
        string PreferredEdition { get; }
        bool EnableVisualStudio { get; }
        TagType TagTypes { get; set; }
        VisualStudioCodeEdition EnabledEditions { get; }
        CommandResultType CommandResult { get; }
        SearchBy SearchBy { get; }
        SecondaryCommand VSSecondaryCommand { get; }
        SecondaryCommand VSCodeSecondaryCommand { get; }
        int PageSize { get; }
        bool EnableLogging { get; }
    }
}