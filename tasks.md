
study README.md in current directory to understand the project
study flow.md in current directory to understand the project
then list all files in Documentation folder and study Documentation/overview.md
you can read other files if you have any question about the extension framework or the helpers

task 1.
in WorkspaceLauncherForVSCode\Pages\VisualStudioCodePage.cs i have TogglePinStatus method
the method is use only inside WorkspaceLauncherForVSCode\Commands\PinWorkspaceCommand.cs
help me to move those code to there if possible, or create service for that, so that VisualStudioCodePage.cs can be cleaner
after complete the task come back here to read task 2.

task 2.
in WorkspaceLauncherForVSCode\Pages\VisualStudioCodePage.cs the method RefreshWorkspacesAsync contains too many lines
and coupled, for example, the method first get data from db then call _vscodeService to pass those db data for setting frequency and lastAccessed
is there any better way to decouple those steps out
after complete the task come back here to read task 3.

task 3.
i have new Sort by option used in WorkspaceLauncherForVSCode\Workspaces\WorkspaceFilter.cs 
        RecentFromVSCode,
        RecentFromVS,
to be in sync with visual studio / code recent list
i need to have a polling like a setinterval function to keep checking if the thing change
for example for RecentFromVSCode, i need to listen to changes in vscdb logic inside WorkspaceLauncherForVSCode\Workspaces\Readers\VscdbWorkspaceReader.cs
it is reading value with key history.recentlyOpenedPathsList
since sqlite does not have subscribe, i have to use interval to keep reading it
if change, i need to update the list
the listening to change should be an opt in for user because it is not realiable, is there any way to make it reliable?