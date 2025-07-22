// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Commands;

internal sealed partial class CopyPathCommand : InvokableCommand
{
    public override string Name => "Copy Path";
    internal string Path { get; }

    internal CopyPathCommand(string path)
    {
        try
        {
            Path = path;
            Icon = new IconInfo("\uE8c8");
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    public override CommandResult Invoke()
    {
        try
        {
            ClipboardHelper.SetText(Path);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
        new ToastStatusMessage($"Copied {Path}").Show();
        return CommandResult.KeepOpen();
    }
}
