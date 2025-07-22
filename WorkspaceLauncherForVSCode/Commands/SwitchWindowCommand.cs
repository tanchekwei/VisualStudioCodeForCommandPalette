// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Components;

namespace WorkspaceLauncherForVSCode.Commands;

internal sealed partial class SwitchWindowCommand : InvokableCommand
{
    private readonly Window _window;

    public SwitchWindowCommand(Window window)
    {
        try
        {
            _window = window;
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
            _window.SwitchToWindow();
            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return CommandResult.KeepOpen();
        }
    }
}
