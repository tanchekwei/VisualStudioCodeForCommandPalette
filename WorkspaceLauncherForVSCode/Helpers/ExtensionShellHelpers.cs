// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace WorkspaceLauncherForVSCode.Helpers;

public static class ExtensionShellHelpers
{
    /// <summary>
    /// Gets the combined machine and user PATH environment variable.
    /// </summary>
    /// <returns>The combined PATH environment variable string.</returns>
    public static string GetUpdatedPath()
    {
        var machinePath = Environment.GetEnvironmentVariable(
            "PATH",
            EnvironmentVariableTarget.Machine);

        var userPath = Environment.GetEnvironmentVariable(
            "PATH",
            EnvironmentVariableTarget.User);

        if (string.IsNullOrWhiteSpace(machinePath))
        {
            return userPath ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(userPath))
        {
            return machinePath;
        }

        return machinePath + Path.PathSeparator + userPath;
    }
}