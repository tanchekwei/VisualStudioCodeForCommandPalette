// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Commands;

internal sealed partial class CopyPathCommand : InvokableCommand
{
    public override string Name => "Copy Path";
    internal string Path { get; }

    internal CopyPathCommand(string path)
    {
        Path = path;
        Icon = Classes.Icon.Copy;
    }

    public override CommandResult Invoke()
    {
        try
        {
            ClipboardHelper.SetText(Path);
        }
        catch
        {
        }
        new ToastStatusMessage($"Copied {Path}").Show();
        return CommandResult.KeepOpen();
    }
}
