// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CmdPal.Ext.System.Helpers;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Components;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Pages;

namespace WorkspaceLauncherForVSCode.Commands;

internal sealed partial class SwitchWindowCommand : InvokableCommand
{
    private readonly Window _window;

    public SwitchWindowCommand(Window window)
    {
        _window = window;
    }

    public override CommandResult Invoke()
    {
        _window.SwitchToWindow();
        return CommandResult.Dismiss();
    }
}
