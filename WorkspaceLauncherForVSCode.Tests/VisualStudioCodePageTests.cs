using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Listeners;
using WorkspaceLauncherForVSCode.Services;
using Microsoft.CommandPalette.Extensions;
using System.Linq;
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class VisualStudioCodePageTests : IDisposable
    {
        private Mock<SettingsManager> _mockSettingsManager = null!;
        private Mock<IVisualStudioCodeService> _mockVsCodeService = null!;
        private Mock<SettingsListener> _mockSettingsListener = null!;
        private Mock<IWorkspaceStorage> _mockWorkspaceStorage = null!;
        private VisualStudioCodePage _page = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSettingsManager = new Mock<SettingsManager>();
            _mockVsCodeService = new Mock<IVisualStudioCodeService>();
            _mockSettingsListener = new Mock<SettingsListener>(_mockSettingsManager.Object);
            _mockWorkspaceStorage = new Mock<IWorkspaceStorage>();

            // Setup mock for GetWorkspacesAsync to return an empty list by default
            _mockWorkspaceStorage.Setup(s => s.GetWorkspacesAsync()).ReturnsAsync(new List<VisualStudioCodeWorkspace>());

            _page = new VisualStudioCodePage(
                _mockSettingsManager.Object,
                _mockVsCodeService.Object,
                _mockSettingsListener.Object,
                _mockWorkspaceStorage.Object);
        }

        public void Dispose()
        {
            _page?.Dispose();
            GC.SuppressFinalize(this);
        }

        [TestMethod]
        public void Constructor_InitializesProperties()
        {
            // Assert
            Assert.IsNotNull(_page);
            Assert.AreEqual("VisualStudioCodePage", _page.Id);
        }

        [TestMethod]
        public async Task StartRefresh_CallsRefreshWorkspacesAsync()
        {
            // Act
            _page.StartRefresh();
            await Task.Delay(100); // Allow async method to be called

            // Assert
            _mockVsCodeService.Verify(s => s.LoadInstances(It.IsAny<Enums.VisualStudioCodeEdition>(), It.IsAny<string>()), Times.AtLeastOnce());
        }

        [TestMethod]
        public void UpdateSearchText_FiltersItems()
        {
            // Arrange
            var mockCommand1 = new Mock<ICommand>();
            var mockWorkspace1 = new VisualStudioCodeWorkspace { Path = "c:\\ws1", Name = "workspace1", WorkspaceName = "workspace1" };
            var mockHasWorkspace1 = mockCommand1.As<IHasWorkspace>();
            mockHasWorkspace1.Setup(x => x.Workspace).Returns(mockWorkspace1);

            var mockCommand2 = new Mock<ICommand>();
            var mockWorkspace2 = new VisualStudioCodeWorkspace { Path = "c:\\ws2", Name = "workspace2", WorkspaceName = "workspace2" };
            var mockHasWorkspace2 = mockCommand2.As<IHasWorkspace>();
            mockHasWorkspace2.Setup(x => x.Workspace).Returns(mockWorkspace2);
            
            var allItems = new List<ListItem>
            {
                new ListItem(mockCommand1.Object) { Title = "workspace1" },
                new ListItem(mockCommand2.Object) { Title = "workspace2" }
            };
            
            _page = new VisualStudioCodePage(
                _mockSettingsManager.Object,
                _mockVsCodeService.Object,
                _mockSettingsListener.Object,
                _mockWorkspaceStorage.Object,
                allItems);

            _mockSettingsManager.Setup(s => s.SearchBy).Returns(Enums.SearchBy.Title);
            _mockSettingsManager.Setup(s => s.PageSize).Returns(10);
            _page.UpdateSearchText("", "workspace1");
            
            var visibleItems = _page.GetItems();
            var filteredItems = visibleItems.Where(x => x.Title != "Still not seeing what you´re looking for?").ToList();

            // Assert
            Assert.AreEqual(1, filteredItems.Count);
            Assert.AreEqual("workspace1", filteredItems[0].Title);
        }

        [TestMethod]
        public void LoadMore_AddsMoreItemsToVisibleList()
        {
            // Arrange
            var allItems = new List<ListItem>();
            for (int i = 0; i < 10; i++)
            {
                var mockCommand = new Mock<ICommand>();
                var mockWorkspace = new VisualStudioCodeWorkspace { Path = $"c:\\ws{i}", Name = $"workspace{i}", WorkspaceName = $"workspace{i}" };
                var mockHasWorkspace = mockCommand.As<IHasWorkspace>();
                mockHasWorkspace.Setup(x => x.Workspace).Returns(mockWorkspace);
                var listItem = new ListItem(mockCommand.Object) { Title = $"workspace{i}" };
                allItems.Add(listItem);
            }
            
            _mockSettingsManager.Setup(s => s.PageSize).Returns(5);
            
            _page = new VisualStudioCodePage(
                _mockSettingsManager.Object,
                _mockVsCodeService.Object,
                _mockSettingsListener.Object,
                _mockWorkspaceStorage.Object,
                allItems);
            
            _page.UpdateSearchText("", "");

            // Act
            _page.LoadMore();
            var visibleItems = _page.GetItems();
            var filteredItems = visibleItems.Where(x => x.Title != "Still not seeing what you´re looking for?").ToList();

            // Assert
            Assert.AreEqual(10, filteredItems.Count);
        }

        [TestMethod]
        public async Task UpdateFrequencyAsync_CallsStorageAndUpdate()
        {
            // Arrange
            var workspace = new VisualStudioCodeWorkspace { Path = "test_path", Frequency = 1 };
            var mockCommand = new Mock<IHasWorkspace>();
            mockCommand.Setup(c => c.Workspace).Returns(workspace);
            var listItem = new ListItem(mockCommand.As<ICommand>().Object) { Title = "test" };

            var allItems = new List<ListItem> { listItem };

            _page = new VisualStudioCodePage(
                _mockSettingsManager.Object,
                _mockVsCodeService.Object,
                _mockSettingsListener.Object,
                _mockWorkspaceStorage.Object,
                allItems);

            _page.UpdateSearchText("", "");


            // Act
            await _page.UpdateFrequencyAsync("test_path");

            // Assert
            _mockWorkspaceStorage.Verify(s => s.UpdateWorkspaceFrequencyAsync("test_path"), Times.Once);
            Assert.AreEqual(2, workspace.Frequency);
        }
    }
}