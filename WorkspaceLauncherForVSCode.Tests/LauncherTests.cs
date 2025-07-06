using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using WorkspaceLauncherForVSCode.Services;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Threading.Tasks;

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
        
        [TestMethod]
        public void Program_Main_RegisterProcessAsComServer()
        {
            // Arrange
            var args = new string[] { "-RegisterProcessAsComServer" };
            
            var task = Task.Run(() => Program.Main(args));

            // Act
            // Give the server some time to start and register the extension
            Task.Delay(500).Wait();

            Program.extensionDisposedEvent?.Set();

            task.Wait(1000);

            // Assert
            Assert.IsTrue(task.IsCompleted);
        }
    }
}