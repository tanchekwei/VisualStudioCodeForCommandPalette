// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Classes
{
    public class RestartCommandPaletteMessage : ToastStatusMessage
    {
        public override int Duration { get; init; } = int.MaxValue;
        public RestartCommandPaletteMessage() : base("Restart Command Palette to apply fallback count changes")
        {
            Message.State = Microsoft.CommandPalette.Extensions.MessageState.Warning;
        }
    }
}
