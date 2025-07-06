using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using WorkspaceLauncherForVSCode.Services;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class LauncherTests
    {
        [TestMethod]
        public void WorkspaceLauncher_Constructor_InitializesProvider()
        {
            // Arrange
            var exitSignal = new ManualResetEvent(false);

            // Act
            var launcher = new WorkspaceLauncherForVSCode(exitSignal);
            var provider = launcher.GetProvider(ProviderType.Commands);

            // Assert
            Assert.IsNotNull(provider);
        }

        [TestMethod]
        public void WorkspaceLauncherCommandsProvider_TopLevelCommands_ReturnsCommands()
        {
            // Arrange
            var provider = new WorkspaceLauncherForVSCodeCommandsProvider();

            // Act
            var commands = provider.TopLevelCommands();

            // Assert
            Assert.IsNotNull(commands);
        }
    }
}