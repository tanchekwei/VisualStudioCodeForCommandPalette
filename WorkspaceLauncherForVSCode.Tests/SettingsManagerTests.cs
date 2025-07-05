using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;
using System.Security.AccessControl;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class SettingsManagerTests
    {
        private SettingsManager _settingsManager = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            // We are creating a new instance for each test to ensure isolation.
            _settingsManager = new SettingsManager();
        }

        [TestMethod]
        public void DefaultValues_AreCorrect()
        {
            // Assert
            Assert.IsFalse(_settingsManager.ShowDetails, "Default for ShowDetails should be false.");
            Assert.AreEqual("Default", _settingsManager.PreferredEdition, "Default for PreferredEdition should be 'Default'.");
            Assert.IsTrue(_settingsManager.EnableVisualStudio, "Default for EnableVisualStudio should be true.");
            Assert.AreEqual(TagType.None, _settingsManager.TagTypes, "Default for TagTypes should be None.");
            Assert.AreEqual(VisualStudioCodeEdition.Default | VisualStudioCodeEdition.System, _settingsManager.EnabledEditions, "Default for EnabledEditions should be Default and System.");
            Assert.AreEqual(CommandResultType.Dismiss, _settingsManager.CommandResult, "Default for CommandResult should be Dismiss.");
            Assert.AreEqual(SearchBy.Title, _settingsManager.SearchBy, "Default for SearchBy should be Title.");
            Assert.AreEqual(SecondaryCommand.OpenAsAdministrator, _settingsManager.VSSecondaryCommand, "Default for VSSecondaryCommand should be OpenAsAdministrator.");
            Assert.AreEqual(SecondaryCommand.OpenInExplorer, _settingsManager.VSCodeSecondaryCommand, "Default for VSCodeSecondaryCommand should be OpenInExplorer.");
            Assert.AreEqual(8, _settingsManager.PageSize, "Default for PageSize should be 8.");
            Assert.IsFalse(_settingsManager.EnableLogging, "Default for EnableLogging should be false.");
        }

        [TestMethod]
        public void TagTypes_CanBeSetAndGet()
        {
            // Arrange
            var expectedTagType = TagType.Type | TagType.Target;

            // Act
            _settingsManager.TagTypes = expectedTagType;
            var actualTagType = _settingsManager.TagTypes;

            // Assert
            Assert.AreEqual(expectedTagType, actualTagType, "TagTypes should be set and get correctly.");
        }
    }
}