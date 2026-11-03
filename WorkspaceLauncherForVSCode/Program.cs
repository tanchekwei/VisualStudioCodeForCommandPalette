// Copyright (c) Jonah Fintz
using System;
using System.Resources;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;
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
                // Use Shmuelie.WinRTServer (same as the official CmdPal extension template).
                // Toolkit ExtensionServer's IClassFactory only accepts IID_IUnknown/CLSID and
                // returns E_NOINTERFACE when CmdPal CoCreateInstance asks for IID_IExtension.
                // See https://github.com/tanchekwei/VisualStudioCodeForCommandPalette/issues/41
                ComServer server = new();
                using var extensionDisposedEvent = new ManualResetEvent(false);

                // Instantiate lazily so COM registration completes before heavy init.
                WorkspaceLauncherForVSCode? extensionInstance = null;
                server.RegisterClass<WorkspaceLauncherForVSCode, IExtension>(
                    () => extensionInstance ??= new WorkspaceLauncherForVSCode(extensionDisposedEvent));
                server.Start();

                extensionDisposedEvent.WaitOne();
                server.Stop();
                server.UnsafeDispose();
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