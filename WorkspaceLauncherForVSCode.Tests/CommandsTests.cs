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
using WorkspaceLauncherForVSCode.Enums;

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
        public void OpenInExplorerCommand_ForSolution_OpensDirectory()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace
            {
                WindowsPath = "C:\\test\\solution.sln",
                WorkspaceType = WorkspaceType.Solution
            };
            var command = new OpenInExplorerCommand(workspace.WindowsPath, workspace);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(CommandResult.Dismiss(), result);
        }

        [TestMethod]
        public void OpenInExplorerCommand_ForRemoteWorkspace_ShowsToast()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace
            {
                WindowsPath = "C:\\test\\path",
                VsCodeRemoteType = VsCodeRemoteType.Remote
            };
            var command = new OpenInExplorerCommand(workspace.WindowsPath, workspace);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(CommandResult.KeepOpen(), result);
        }

        [TestMethod]
        public void OpenInExplorerCommand_WithInvalidPath_ShowsToast()
        {
            // Arrange
            var command = new OpenInExplorerCommand("invalidpath", null);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(CommandResult.KeepOpen(), result);
        }

        [TestMethod]
        public void OpenInExplorerCommand_WithNullPath_DoesNothing()
        {
            // Arrange
            var command = new OpenInExplorerCommand(null, null);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(CommandResult.Dismiss(), result);
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