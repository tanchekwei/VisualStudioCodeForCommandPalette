// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.IO;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Services
{
    public static class VisualStudioCodeInstanceProvider
    {
        public static List<VisualStudioCodeInstance> GetInstances(VisualStudioCodeEdition enabledEditions)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            var instances = new List<VisualStudioCodeInstance>();
            LoadInstances(enabledEditions, instances);
            return instances;
        }

        private static void LoadInstances(VisualStudioCodeEdition enabledEditions, List<VisualStudioCodeInstance> instances)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            var appdataProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var programsFolderPathBase = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var defaultStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code", "User", "globalStorage");
            var insiderStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Code - Insiders", "User", "globalStorage");

            if (enabledEditions.HasFlag(VisualStudioCodeEdition.Default))
            {
                AddInstance(instances, "VS Code", Path.Combine(appdataProgramFilesPath, "Programs", "Microsoft VS Code", "Code.exe"), defaultStoragePath, VisualStudioCodeInstallationType.User, VisualStudioCodeType.Default);
            }
            if (enabledEditions.HasFlag(VisualStudioCodeEdition.System))
            {
                AddInstance(instances, "VS Code [System]", Path.Combine(programsFolderPathBase, "Microsoft VS Code", "Code.exe"), defaultStoragePath, VisualStudioCodeInstallationType.System, VisualStudioCodeType.Default);
            }
            if (enabledEditions.HasFlag(VisualStudioCodeEdition.Insider))
            {
                AddInstance(instances, "VS Code - Insiders", Path.Combine(appdataProgramFilesPath, "Programs", "Microsoft VS Code Insiders", "Code - Insiders.exe"), insiderStoragePath, VisualStudioCodeInstallationType.User, VisualStudioCodeType.Insider);
                AddInstance(instances, "VS Code - Insiders [System]", Path.Combine(programsFolderPathBase, "Microsoft VS Code Insiders", "Code - Insiders.exe"), insiderStoragePath, VisualStudioCodeInstallationType.System, VisualStudioCodeType.Insider);
            }
            if (enabledEditions.HasFlag(VisualStudioCodeEdition.Custom))
            {
                try
                {
                    var pathEnv = Environment.GetEnvironmentVariable("PATH");
                    if (!string.IsNullOrEmpty(pathEnv))
                    {
                        var paths = pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var dir in paths)
                        {
                            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                            {
                                continue;
                            }
                            var parentDir = Path.GetDirectoryName(dir) ?? dir;
                            try
                            {
                                var codeExe = Path.Combine(parentDir, "code.exe");
                                var codeInsidersExe = Path.Combine(parentDir, "Code - Insiders.exe");

                                if (File.Exists(codeExe))
                                {
                                    AddInstance(instances, "VS Code [Custom]", codeExe, defaultStoragePath, VisualStudioCodeInstallationType.User, VisualStudioCodeType.Default);
                                }
                                if (File.Exists(codeInsidersExe))
                                {
                                    AddInstance(instances, "VS Code - Insiders [Custom]", codeInsidersExe, insiderStoragePath, VisualStudioCodeInstallationType.User, VisualStudioCodeType.Insider);
                                }
                            }
                            catch
                            {
                                // ignore any errors while checking for custom installations
                            }
                        }
                    }
                }
                catch
                {
                    // ignore invalid PATH entries
                }
            }
        }

        private static void AddInstance(List<VisualStudioCodeInstance> instances, string name, string path, string storagePath, VisualStudioCodeInstallationType type, VisualStudioCodeType codeType)
        {
            if (File.Exists(path))
            {
                if (instances.Exists(instance => instance.ExecutablePath.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }
                instances.Add(new VisualStudioCodeInstance(name, path, storagePath, type, codeType));
            }
        }
    }
}