using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.CommandPalette.Extensions;
using WorkspaceLauncherForVSCode.Workspaces;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Listeners;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class WorkspaceItemFactoryTests
    {
        private Mock<VisualStudioCodePage> _mockVisualStudioCodePage;
        private Mock<WorkspaceStorage> _mockWorkspaceStorage;
        private Mock<SettingsManager> _mockSettingsManager;
        private CommandContextItem _refreshCommandContextItem;
        private CommandContextItem _helpCommandContextItem;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSettingsManager = new Mock<SettingsManager>();
            var mockVsCodeService = new Mock<IVisualStudioCodeService>();
            var mockSettingsListener = new Mock<SettingsListener>(_mockSettingsManager.Object);
            _mockVisualStudioCodePage = new Mock<VisualStudioCodePage>(_mockSettingsManager.Object, mockVsCodeService.Object, mockSettingsListener.Object);
            _mockWorkspaceStorage = new Mock<WorkspaceStorage>();
            _refreshCommandContextItem = new CommandContextItem(new Mock<ICommand>().Object);
            _helpCommandContextItem = new CommandContextItem(new Mock<ICommand>().Object);
        }

        [TestMethod]
        public void Create_ForSolution_ReturnsCorrectListItem()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace
            {
                WorkspaceType = WorkspaceType.Solution,
                Name = "Test Solution",
                Path = "C:\\solutions\\test.sln",
                WindowsPath = "C:\\solutions\\test.sln"
            };

            _mockSettingsManager.Setup(s => s.VSSecondaryCommand).Returns(SecondaryCommand.OpenInExplorer);

            // Act
            var listItem = WorkspaceItemFactory.Create(workspace, _mockVisualStudioCodePage.Object, _mockWorkspaceStorage.Object, _mockSettingsManager.Object, _refreshCommandContextItem, _helpCommandContextItem);

            // Assert
            Assert.AreEqual("Test Solution", listItem.Title);
            Assert.AreEqual("C:\\solutions\\test.sln", listItem.Subtitle);
            Assert.IsInstanceOfType(listItem.Command, typeof(Commands.OpenSolutionCommand));
        }

        [TestMethod]
        public void Create_ForVSCodeWorkspace_ReturnsCorrectListItem()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace
            {
                WorkspaceType = WorkspaceType.Workspace,
                WorkspaceName = "Test Workspace",
                Path = "C:\\workspaces\\test.code-workspace",
                WindowsPath = "C:\\workspaces\\test.code-workspace"
            };

            _mockSettingsManager.Setup(s => s.VSCodeSecondaryCommand).Returns(SecondaryCommand.OpenInExplorer);

            // Act
            var listItem = WorkspaceItemFactory.Create(workspace, _mockVisualStudioCodePage.Object, _mockWorkspaceStorage.Object, _mockSettingsManager.Object, _refreshCommandContextItem, _helpCommandContextItem);

            // Assert
            Assert.AreEqual("Test Workspace", listItem.Title);
            Assert.AreEqual("C:\\workspaces\\test.code-workspace", listItem.Subtitle);
            Assert.IsInstanceOfType(listItem.Command, typeof(Commands.OpenVisualStudioCodeCommand));
        }
    }
}