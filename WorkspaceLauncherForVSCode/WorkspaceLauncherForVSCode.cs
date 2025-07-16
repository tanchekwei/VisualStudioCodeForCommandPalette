// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using Microsoft.Extensions.DependencyInjection;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Pages;
using WorkspaceLauncherForVSCode.Services;
using WorkspaceLauncherForVSCode.Services.VisualStudio;

namespace WorkspaceLauncherForVSCode;

#if DEBUG
[Guid("4c23de8f-6bdd-41a1-92f0-744d7af84659")]
#else
    [Guid("c6506a70-a0c8-4a96-9bc8-29714d6b2e34")]
#endif
public sealed partial class WorkspaceLauncherForVSCode : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private readonly WorkspaceLauncherForVSCodeCommandsProvider _provider;

    public WorkspaceLauncherForVSCode(ManualResetEvent extensionDisposedEvent)
    {
        this._extensionDisposedEvent = extensionDisposedEvent;
        var services = new ServiceCollection();

        // Register dependencies
        services.AddSingleton<SettingsManager>();
        services.AddSingleton<IVisualStudioCodeService, VisualStudioCodeService>();
        services.AddSingleton<VisualStudioService>();
        services.AddSingleton<SettingsListener>();
        services.AddSingleton<VisualStudioCodePage>();
        services.AddSingleton<WorkspaceStorage>();
        services.AddSingleton<RefreshWorkspacesCommand>();
        services.AddSingleton<CountTracker>();
        services.AddSingleton<IPinService, PinService>();
        services.AddSingleton<IVSCodeWorkspaceWatcherService, VSCodeWorkspaceWatcherService>();
        services.AddSingleton<IVSWorkspaceWatcherService, VSWorkspaceWatcherService>();
        services.AddSingleton<WorkspaceLauncherForVSCodeCommandsProvider>();

        // Build the provider
        var provider = services.BuildServiceProvider();

        StaticHelpItems.Initialize(provider.GetRequiredService<SettingsManager>(), provider.GetRequiredService<CountTracker>());

        _provider = provider.GetRequiredService<WorkspaceLauncherForVSCodeCommandsProvider>();
    }

    public object? GetProvider(ProviderType providerType)
    {
#if DEBUG
        using var logger = new TimeLogger();
#endif
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    public void Dispose() => this._extensionDisposedEvent.Set();
}
