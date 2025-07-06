using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkspaceLauncherForVSCode.Workspaces;
using WorkspaceLauncherForVSCode.Enums;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class WorkspaceFilterTests
    {
        [TestMethod]
        public void Filter_ByTitle_ReturnsCorrectResults()
        {
            // Arrange
            var items = new List<ListItem>
            {
                new ListItem(new Moq.Mock<ICommand>().Object) { Title = "Test Solution" },
                new ListItem(new Moq.Mock<ICommand>().Object) { Title = "Another Workspace" }
            };

            // Act
            var result = WorkspaceFilter.Filter("Solution", items, SearchBy.Title);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test Solution", result[0].Title);
        }

        [TestMethod]
        public void Filter_ByPath_ReturnsCorrectResults()
        {
            // Arrange
            var items = new List<ListItem>
            {
                new ListItem(new Moq.Mock<ICommand>().Object) { Subtitle = "C:\\solutions\\test.sln" },
                new ListItem(new Moq.Mock<ICommand>().Object) { Subtitle = "C:\\workspaces\\another.code-workspace" }
            };

            // Act
            var result = WorkspaceFilter.Filter("workspaces", items, SearchBy.Path);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("C:\\workspaces\\another.code-workspace", result[0].Subtitle);
        }

        [TestMethod]
        public void Filter_ByBoth_ReturnsCorrectResults()
        {
            // Arrange
            var items = new List<ListItem>
            {
                new ListItem(new Moq.Mock<ICommand>().Object) { Title = "Test Solution", Subtitle = "C:\\solutions\\test.sln" },
                new ListItem(new Moq.Mock<ICommand>().Object) { Title = "Another Workspace", Subtitle = "C:\\workspaces\\another.code-workspace" }
            };

            // Act
            var result = WorkspaceFilter.Filter("another", items, SearchBy.Both);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Another Workspace", result[0].Title);
        }
    }
}