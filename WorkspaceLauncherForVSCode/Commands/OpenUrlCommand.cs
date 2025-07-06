// Copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Commands
{
    public sealed partial class OpenUrlCommand : InvokableCommand
    {
        private readonly string Url;

        public OpenUrlCommand(string url, string name, IconInfo icon)
        {
            Url = url;
            Name = name;
            Icon = icon;
        }

        public override ICommandResult Invoke()
        {
            Process.Start(new ProcessStartInfo(Url) { UseShellExecute = true });
            return CommandResult.Hide();
        }
    }
}