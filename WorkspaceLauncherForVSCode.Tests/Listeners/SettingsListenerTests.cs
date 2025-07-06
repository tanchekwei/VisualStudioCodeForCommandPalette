using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Listeners;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;

namespace WorkspaceLauncherForVSCode.Tests.Listeners
{
    [TestClass]
    public class SettingsListenerTests
    {
        private Mock<SettingsManager> _mockSettingsManager = null!;
        private SettingsListener _listener = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSettingsManager = new Mock<SettingsManager>();
            _listener = new SettingsListener(_mockSettingsManager.Object);
        }
    }
}