using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Classes;
using System;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Listeners;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class PinWorkspaceCommandTests
    {
        private Mock<VisualStudioCodePage> _mockPage;
        private Mock<WorkspaceStorage> _mockStorage;
        private Mock<SettingsManager> _mockSettingsManager;
        private Mock<IVisualStudioCodeService> _mockVsCodeService;
        private Mock<SettingsListener> _mockSettingsListener;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSettingsManager = new Mock<SettingsManager>();
            _mockVsCodeService = new Mock<IVisualStudioCodeService>();
            _mockSettingsListener = new Mock<SettingsListener>(_mockSettingsManager.Object);
            _mockPage = new Mock<VisualStudioCodePage>(_mockSettingsManager.Object, _mockVsCodeService.Object, _mockSettingsListener.Object);
            _mockStorage = new Mock<WorkspaceStorage>();
        }

        [TestMethod]
        public void Name_WhenWorkspaceIsPinned_ReturnsUnpinFromList()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace { PinDateTime = DateTime.UtcNow };
            var command = new PinWorkspaceCommand(workspace, _mockPage.Object, _mockStorage.Object);

            // Assert
            Assert.AreEqual("Unpin from List", command.Name);
        }

        [TestMethod]
        public void Name_WhenWorkspaceIsNotPinned_ReturnsPinToList()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace();
            var command = new PinWorkspaceCommand(workspace, _mockPage.Object, _mockStorage.Object);

            // Assert
            Assert.AreEqual("Pin to List", command.Name);
        }

        [TestMethod]
        public async Task Invoke_WhenWorkspaceIsPinned_CallsRemovePinnedWorkspaceAsync()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace { Path = "test_path", PinDateTime = DateTime.UtcNow };
            var command = new PinWorkspaceCommand(workspace, _mockPage.Object, _mockStorage.Object);

            // Act
            command.Invoke();
            await Task.Delay(100); // Wait for the async task to complete

            // Assert
            _mockStorage.Verify(s => s.RemovePinnedWorkspaceAsync(workspace.Path), Times.Once);
        }

        [TestMethod]
        public async Task Invoke_WhenWorkspaceIsNotPinned_CallsAddPinnedWorkspaceAsync()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace { Path = "test_path" };
            var command = new PinWorkspaceCommand(workspace, _mockPage.Object, _mockStorage.Object);

            // Act
            command.Invoke();
            await Task.Delay(100); // Wait for the async task to complete

            // Assert
            _mockStorage.Verify(s => s.AddPinnedWorkspaceAsync(workspace.Path), Times.Once);
        }
    }
}