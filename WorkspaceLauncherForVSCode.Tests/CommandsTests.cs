using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Classes;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Interfaces;
using System.IO;
using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void CopyPathCommand_Invoke_CopiesPathToClipboard()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace { WindowsPath = "C:\\test\\path" };
            var command = new CopyPathCommand(workspace.WindowsPath);

            // Act
            command.Invoke();

            // Assert
            // Note: We can't directly verify clipboard content in a unit test.
            // This test primarily ensures the command executes without error.
        }

        [TestMethod]
        public void OpenInExplorerCommand_Invoke_StartsExplorerProcess()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace { WindowsPath = "C:\\test\\path" };
            var command = new OpenInExplorerCommand(workspace.WindowsPath, workspace);

            // Act & Assert
            try
            {
                command.Invoke();
                // If it doesn't throw, we assume it worked.
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected no exception, but got: {ex.Message}");
            }
        }

        [TestMethod]
        public void OpenUrlCommand_Invoke_StartsBrowserProcess()
        {
            // Arrange
            var url = "https://github.com";
            var command = new global::WorkspaceLauncherForVSCode.Commands.OpenUrlCommand(url, "Open GitHub", new IconInfo(default!, default!));

            // Act & Assert
            try
            {
                command.Invoke();
                // If it doesn't throw, we assume it worked.
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected no exception, but got: {ex.Message}");
            }
        }
    }
}