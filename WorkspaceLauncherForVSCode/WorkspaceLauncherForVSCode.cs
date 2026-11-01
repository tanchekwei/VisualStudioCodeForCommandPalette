// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
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
    private readonly VisualStudioCodePage _page;
    private readonly WorkspaceLauncherForVSCodeCommandsProvider _provider;

    public WorkspaceLauncherForVSCode(ManualResetEvent extensionDisposedEvent)
    {
        try
        {
            this._extensionDisposedEvent = extensionDisposedEvent;

            var settingsManager = new SettingsManager();
            var visualStudioService = new VisualStudioService();
            var vscodeService = new VisualStudioCodeService(visualStudioService);
            var settingsListener = new SettingsListener(settingsManager);
            var workspaceStorage = new WorkspaceStorage();
            var refreshWorkspacesCommand = new RefreshWorkspacesCommand(vscodeService, settingsManager);
            var pinService = new PinService(workspaceStorage);
            var vscodeWatcherService = new VSCodeWorkspaceWatcherService(vscodeService, settingsManager);
            var vsWatcherService = new VSWorkspaceWatcherService(settingsManager, visualStudioService);

            _page = new VisualStudioCodePage(
                settingsManager,
                vscodeService,
                settingsListener,
                workspaceStorage,
                refreshWorkspacesCommand,
                vscodeWatcherService,
                vsWatcherService,
                pinService);

            StaticHelpItems.Initialize(settingsManager, workspaceStorage, _page.StartRefresh);

            _provider = new WorkspaceLauncherForVSCodeCommandsProvider(
                settingsManager,
                settingsListener,
                _page);
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            throw;
        }
    }

    public object? GetProvider(ProviderType providerType)
    {
        try
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
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return null;
        }
    }

    public void Dispose()
    {
        try
        {
            _page?.Dispose();
            this._extensionDisposedEvent.Set();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }
}