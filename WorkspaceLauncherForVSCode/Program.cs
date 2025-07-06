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
    internal static ManualResetEvent? extensionDisposedEvent;

    [MTAThread]
    public static void Main(string[] args)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
        {
            using ExtensionServer server = new();
            extensionDisposedEvent = new ManualResetEvent(false);

            // We are instantiating an extension instance once above, and returning it every time the callback in RegisterExtension below is called.
            // This makes sure that only one instance of SampleExtension is alive, which is returned every time the host asks for the IExtension object.
            // If you want to instantiate a new instance each time the host asks, create the new instance inside the delegate.
            var extensionInstance = new WorkspaceLauncherForVSCode(extensionDisposedEvent);
            server.RegisterExtension(() => extensionInstance);
            // This will make the main thread wait until the event is signalled by the extension class.
            // Since we have single instance of the extension object, we exit as soon as it is disposed.
            extensionDisposedEvent.WaitOne();
        }
        else
        {
            Console.WriteLine("Not being launched as a Extension... exiting.");
        }
    }
}
