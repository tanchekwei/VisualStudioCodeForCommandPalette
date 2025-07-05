using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;
using WorkspaceLauncherForVSCode.Services;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class VisualStudioCodeServiceTests
    {
        private Mock<IVisualStudioCodeInstanceProvider> _mockInstanceProvider = null!;
        private Mock<IVisualStudioCodeWorkspaceProvider> _mockWorkspaceProvider = null!;
        private Mock<IVisualStudioProvider> _mockVisualStudioProvider = null!;
        private VisualStudioCodeService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockInstanceProvider = new Mock<IVisualStudioCodeInstanceProvider>();
            _mockWorkspaceProvider = new Mock<IVisualStudioCodeWorkspaceProvider>();
            _mockVisualStudioProvider = new Mock<IVisualStudioProvider>();
            _service = new VisualStudioCodeService(
                _mockInstanceProvider.Object,
                _mockWorkspaceProvider.Object,
                _mockVisualStudioProvider.Object);
        }

        [TestMethod]
        public void LoadInstances_CallsGetInstances()
        {
            // Arrange
            var enabledEditions = VisualStudioCodeEdition.Default;
            var preferredEdition = "Default";

            // Act
            _service.LoadInstances(enabledEditions, preferredEdition);

            // Assert
            _mockInstanceProvider.Verify(p => p.GetInstances(enabledEditions, preferredEdition), Times.Once);
        }

        [TestMethod]
        public async Task GetWorkspacesAsync_ReturnsCombinedWorkspaces()
        {
            // Arrange
            var instances = new List<VisualStudioCodeInstance> { new VisualStudioCodeInstance("test", "test", "test", VisualStudioCodeInstallationType.User, VisualStudioCodeType.Default) };
            _mockInstanceProvider.Setup(p => p.GetInstances(It.IsAny<VisualStudioCodeEdition>(), It.IsAny<string>())).Returns(instances);
            _service.LoadInstances(VisualStudioCodeEdition.Default, "Default");

            var workspace1 = new VisualStudioCodeWorkspace { Path = "/path/to/workspace1", Source = VisualStudioCodeWorkspaceSource.StorageJson };
            var workspace2 = new VisualStudioCodeWorkspace { Path = "/path/to/workspace2", Source = VisualStudioCodeWorkspaceSource.Vscdb };
            var workspaces = new List<VisualStudioCodeWorkspace> { workspace1, workspace2 };

            _mockWorkspaceProvider
                .Setup(p => p.GetWorkspacesAsync(It.IsAny<VisualStudioCodeInstance>(), It.IsAny<List<VisualStudioCodeWorkspace>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(workspaces);

            // Act
            var result = await _service.GetWorkspacesAsync(new List<VisualStudioCodeWorkspace>(), CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetVisualStudioSolutions_CallsGetSolutions()
        {
            // Arrange
            var dbWorkspaces = new List<VisualStudioCodeWorkspace>();
            var showPrerelease = false;

            // Act
            await _service.GetVisualStudioSolutions(dbWorkspaces, showPrerelease);

            // Assert
            _mockVisualStudioProvider.Verify(p => p.GetSolutions(dbWorkspaces, showPrerelease), Times.Once);
        }
    }
}