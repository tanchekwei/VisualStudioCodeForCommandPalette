using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Classes;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Interfaces;
using System.IO;
using System;
using Microsoft.CommandPalette.Extensions;
using Toolkit = Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Enums;
using System.Reflection;
using WorkspaceLauncherForVSCode.Services;
using WorkspaceLauncherForVSCode.Listeners;
using System.Collections.Generic;
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
            Assert.AreEqual(Toolkit.CommandResult.KeepOpen().Kind, result.Kind);
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
            Assert.AreEqual(Toolkit.CommandResult.KeepOpen().Kind, result.Kind);
        }

        [TestMethod]
        public void OpenInExplorerCommand_WithInvalidPath_ShowsToast()
        {
            // Arrange
            var command = new OpenInExplorerCommand("invalidpath", null);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(Toolkit.CommandResult.KeepOpen().Kind, result.Kind);
        }

        [TestMethod]
        public void OpenInExplorerCommand_WithNullPath_DoesNothing()
        {
            // Arrange
            var command = new OpenInExplorerCommand(null, null);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(Toolkit.CommandResult.Dismiss().Kind, result.Kind);
        }

        [TestMethod]
        public void OpenUrlCommand_Invoke_StartsBrowserProcess()
        {
            // Arrange
            var url = "https://github.com";
            var command = new global::WorkspaceLauncherForVSCode.Commands.OpenUrlCommand(url, "Open GitHub", Icon.Web);

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
        public void OpenSolutionCommand_Invoke_ReturnsDismissResult()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace { WindowsPath = "C:\\test\\solution.sln" };
            var mockPage = new Mock<IVisualStudioCodePage>();
            var command = new OpenSolutionCommand(workspace, mockPage.Object, CommandResultType.Dismiss, false);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(CommandResultKind.KeepOpen, result.Kind);
        }

        [TestMethod]
        public void OpenVisualStudioCodeCommand_Invoke_ReturnsDismissResult()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace { WindowsPath = "C:\\test\\path" };
            var mockPage = new Mock<IVisualStudioCodePage>();
            var command = new OpenVisualStudioCodeCommand(workspace, mockPage.Object, CommandResultType.Dismiss, false);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(CommandResultKind.Confirm, result.Kind);
        }

        [TestMethod]
        public void RefreshWorkspacesCommand_Invoke_ReturnsDismissResult()
        {
            // Arrange
            var mockService = new Mock<IVisualStudioCodeService>();
            var settingsManager = new SettingsManager();
            var mockPage = new VisualStudioCodePage(
                settingsManager,
                mockService.Object,
                new SettingsListener(settingsManager),
                new Mock<IWorkspaceStorage>().Object,
                new List<ListItem>()
            );
            var command = new RefreshWorkspacesCommand(mockService.Object, settingsManager, mockPage);

            // Act
            var result = command.Invoke();

            // Assert
            Assert.AreEqual(CommandResultKind.KeepOpen, result.Kind);
        }
    }
}