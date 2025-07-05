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
    public class VisualStudioCodePageTests
    {
        private Mock<SettingsManager> _mockSettingsManager;
        private Mock<IVisualStudioCodeService> _mockVsCodeService;
        private Mock<SettingsListener> _mockSettingsListener;
        private Mock<IWorkspaceStorage> _mockWorkspaceStorage;
        private VisualStudioCodePage _page;

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
            var allItems = new List<ListItem>
            {
                new ListItem(new Mock<ICommand>().Object) { Title = "workspace1" },
                new ListItem(new Mock<ICommand>().Object) { Title = "workspace2" }
            };
            _page.ClearAllItems();
            _page.GetType().GetField("_allItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_page, allItems);


            // Act
            _page.UpdateSearchText("", "workspace1");

            // Assert
            Assert.AreEqual(1, _page.GetItems().Length);
            Assert.AreEqual("workspace1", _page.GetItems()[0].Title);
        }

        [TestMethod]
        public void LoadMore_AddsMoreItemsToVisibleList()
        {
            // Arrange
            var allItems = new List<ListItem>();
            for (int i = 0; i < 10; i++)
            {
                var mockCommand = new Mock<ICommand>();
                var listItem = new ListItem(mockCommand.Object) { Title = $"workspace{i}" };
                allItems.Add(listItem);
            }
            _page.ClearAllItems();
            _page.GetType().GetField("_allItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_page, allItems);
            _mockSettingsManager.Setup(s => s.PageSize).Returns(5);
            _page.UpdateSearchText("", "");

            // Act
            _page.LoadMore();

            // Assert
            Assert.AreEqual(10, _page.GetItems().Length-1);
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
            _page.ClearAllItems();
            _page.GetType().GetField("_allItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_page, allItems);
            _page.UpdateSearchText("", "");


            // Act
            await _page.UpdateFrequencyAsync("test_path");

            // Assert
            _mockWorkspaceStorage.Verify(s => s.UpdateWorkspaceFrequencyAsync("test_path"), Times.Once);
            Assert.AreEqual(2, workspace.Frequency);
        }
    }

    public interface IHasWorkspace
    {
        VisualStudioCodeWorkspace Workspace { get; }
    }
}