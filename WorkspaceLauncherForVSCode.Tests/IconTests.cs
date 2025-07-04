using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class IconTests
    {
        [TestMethod]
        public void ExtensionIconIsNotNull()
        {
            Assert.IsNotNull(Icon.Extension);
        }
    }
}