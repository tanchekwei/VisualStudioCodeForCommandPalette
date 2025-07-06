using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Commands;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class PinWorkspaceCommandTests
    {
        private Mock<IVisualStudioCodePage> _mockPage = null!;
        private Mock<IWorkspaceStorage> _mockWorkspaceStorage = null!;
        private VisualStudioCodeWorkspace _workspace = null!;
        private PinWorkspaceCommand _command = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockPage = new Mock<IVisualStudioCodePage>();
            _mockWorkspaceStorage = new Mock<IWorkspaceStorage>();
            _workspace = new VisualStudioCodeWorkspace { Path = "test_path" };
            _command = new PinWorkspaceCommand(_workspace, _mockPage.Object, _mockWorkspaceStorage.Object);
        }

        [TestMethod]
        public void Name_WhenWorkspaceIsPinned_ReturnsUnpinFromList()
        {
            // Arrange
            _workspace.PinDateTime = System.DateTime.UtcNow;

            // Assert
            Assert.AreEqual("Unpin from List", _command.Name);
        }

        [TestMethod]
        public void Name_WhenWorkspaceIsNotPinned_ReturnsPinToList()
        {
            // Arrange
            _workspace.PinDateTime = null;

            // Assert
            Assert.AreEqual("Pin to List", _command.Name);
        }

        [TestMethod]
        public async Task Invoke_WhenWorkspaceIsPinned_CallsRemovePinnedWorkspaceAsync()
        {
            // Arrange
            _workspace.PinDateTime = System.DateTime.UtcNow;

            // Act
            _command.Invoke();
            await Task.Delay(100);

            // Assert
            _mockWorkspaceStorage.Verify(s => s.RemovePinnedWorkspaceAsync("test_path"), Times.Once);
            if (_mockPage.Object is VisualStudioCodePage concretePage)
            {
                _mockPage.As<IVisualStudioCodePage>().Verify(p => concretePage.TogglePinStatus("test_path"), Times.Once);
            }
        }

        [TestMethod]
        public async Task Invoke_WhenWorkspaceIsNotPinned_CallsAddPinnedWorkspaceAsync()
        {
            // Arrange
            _workspace.PinDateTime = null;

            // Act
            _command.Invoke();
            await Task.Delay(100);

            // Assert
            _mockWorkspaceStorage.Verify(s => s.AddPinnedWorkspaceAsync("test_path"), Times.Once);
            if (_mockPage.Object is VisualStudioCodePage concretePage)
            {
                _mockPage.As<IVisualStudioCodePage>().Verify(p => concretePage.TogglePinStatus("test_path"), Times.Once);
            }
        }
    }
}