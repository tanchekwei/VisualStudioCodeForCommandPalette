// Copyright (c) Jonah Fintz
using System;
using System.Resources;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using WorkspaceLauncherForVSCode.Classes;

[assembly: NeutralResourcesLanguage("en-US")]

namespace WorkspaceLauncherForVSCode;

public class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        try
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            if (args.Length > 0 && string.Equals(args[0], "-RegisterProcessAsComServer", StringComparison.Ordinal))
            {
                using ExtensionServer server = new();
                using var extensionDisposedEvent = new ManualResetEvent(false);

                // Instantiate extension lazily when the host first asks for the IExtension object.
                // This ensures COM server registration completes immediately without waiting for heavy initialization.
                WorkspaceLauncherForVSCode? extensionInstance = null;
                server.RegisterExtension(() => extensionInstance ??= new WorkspaceLauncherForVSCode(extensionDisposedEvent));

                // Wait until the event is signaled by the extension class upon disposal.
                extensionDisposedEvent.WaitOne();
                extensionInstance?.Dispose();
            }
            else
            {
                string productName = Constant.VisualStudioCodeDisplayName;
                NativeDialog.Show(
                    productName,
                    $"{productName} is a PowerToys Command Palette extension.\n\nInstall and enable PowerToys Command Palette,\n\nthen open {productName} extension from Command Palette.");
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }
}