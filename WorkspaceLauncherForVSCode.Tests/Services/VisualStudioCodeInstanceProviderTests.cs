using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Services;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Tests.Services
{
    [TestClass]
    public class VisualStudioCodeInstanceProviderTests
    {
        [TestMethod]
        public void GetInstances_WithDefaultEdition_ReturnsCorrectInstances()
        {
            // Arrange
            var enabledEditions = VisualStudioCodeEdition.Default;
            var preferredEdition = "Default";

            // Act
            var result = VisualStudioCodeInstanceProvider.GetInstances(enabledEditions, preferredEdition);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetInstances_WithInsiderEdition_ReturnsInsiderInstances()
        {
            // Arrange
            var enabledEditions = VisualStudioCodeEdition.Insider;
            var preferredEdition = "Insider";

            // Act
            var result = VisualStudioCodeInstanceProvider.GetInstances(enabledEditions, preferredEdition);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.All(i => i.VisualStudioCodeType == VisualStudioCodeType.Insider));
        }

        [TestMethod]
        public void GetInstances_WithAllEditions_ReturnsAllInstances()
        {
            // Arrange
            var enabledEditions = VisualStudioCodeEdition.Default | VisualStudioCodeEdition.System | VisualStudioCodeEdition.Insider | VisualStudioCodeEdition.Custom;
            var preferredEdition = "Default";

            // Act
            var result = VisualStudioCodeInstanceProvider.GetInstances(enabledEditions, preferredEdition);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetInstances_WithNoEditions_ReturnsEmptyList()
        {
            // Arrange
            var enabledEditions = VisualStudioCodeEdition.None;
            var preferredEdition = "Default";

            // Act
            var result = VisualStudioCodeInstanceProvider.GetInstances(enabledEditions, preferredEdition);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetInstances_WithPreferredEditionAsInsider_SortsCorrectly()
        {
            // Arrange
            var enabledEditions = VisualStudioCodeEdition.Default | VisualStudioCodeEdition.System | VisualStudioCodeEdition.Insider | VisualStudioCodeEdition.Custom;
            var preferredEdition = "Insider";

            // Act
            var result = VisualStudioCodeInstanceProvider.GetInstances(enabledEditions, preferredEdition);

            // Assert
            Assert.IsNotNull(result);
            if (result.Count > 1)
            {
                Assert.IsTrue(result[0].VisualStudioCodeType == VisualStudioCodeType.Insider);
            }
        }
    }
}