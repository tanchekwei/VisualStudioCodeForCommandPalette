using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkspaceLauncherForVSCode.Helpers;

namespace WorkspaceLauncherForVSCode.Tests
{
    [TestClass]
    public class HelpersTests
    {
        [DataTestMethod]
        [DataRow("file:///c:/Users/user/project", "C:\\Users\\user\\project")]
        [DataRow("file:///C:/Users/user/project", "C:\\Users\\user\\project")]
        [DataRow("vscode-remote://wsl%2BUbuntu/home/user/project", @"\\wsl$\Ubuntu\home\user\project")]
        [DataRow("file://wsl.localhost/Ubuntu/home/user/project", "\\\\wsl.localhost\\Ubuntu\\home\\user\\project")]
        [DataRow(@"\\wsl$\Ubuntu\home\user\project", @"\\wsl$\Ubuntu\home\user\project")]
        [DataRow(@"\\wsl.localhost\Ubuntu\home\user\project", @"\\wsl.localhost\Ubuntu\home\user\project")]
        [DataRow("c:\\Users\\user\\project", "C:\\Users\\user\\project")]
        public void TryConvertToWindowsPath_ConvertsValidUris(string input, string expected)
        {
            // Act
            var result = FileUriParser.TryConvertToWindowsPath(input, out var windowsPath);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(expected, windowsPath);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void TryConvertToWindowsPath_ReturnsFalseForInvalidInput(string input)
        {
            // Act
            var result = FileUriParser.TryConvertToWindowsPath(input, out var windowsPath);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(windowsPath);
        }
    }
}