// Modifications copyright (c) 2025 tanchekwei 
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Services
{
    public static class VisualStudioCodeInstanceProvider
    {
        public static async Task<List<VisualStudioCodeInstance>> GetInstancesAsync(VisualStudioCodeEdition enabledEditions, string? cursorPath = null, string? antigravityPath = null)
        {
            return await Task.Run(() => GetInstances(enabledEditions, cursorPath, antigravityPath));
        }

        public static List<VisualStudioCodeInstance> GetInstances(VisualStudioCodeEdition enabledEditions, string? cursorPath = null, string? antigravityPath = null)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            var instances = new List<VisualStudioCodeInstance>();
            try
            {
                LoadInstances(enabledEditions, instances, cursorPath, antigravityPath);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
            return instances;
        }

        private static void LoadInstances(VisualStudioCodeEdition enabledEditions, List<VisualStudioCodeInstance> instances, string? cursorPathOverride = null, string? antigravityPathOverride = null)
        {
#if DEBUG
            using var logger = new TimeLogger();
#endif
            try
            {
                var appdataProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var programsFolderPathBase = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var appDataBasePath = Environment.GetEnvironmentVariable("VSCODE_APPDATA") ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var defaultStoragePath = Path.Combine(appDataBasePath, "Code", "User", "globalStorage");
                var insiderStoragePath = Path.Combine(appDataBasePath, "Code - Insiders", "User", "globalStorage");

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
                                try
                                {
                                    if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                                    {
                                        continue;
                                    }
                                    var parentDir = Path.GetDirectoryName(dir) ?? dir;
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
                                catch (Exception ex)
                                {
                                    ErrorLogger.LogError(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex);
                    }
                }
                if (enabledEditions.HasFlag(VisualStudioCodeEdition.Cursor))
                {
                    var cursorStoragePath = Path.Combine(appDataBasePath, "Cursor", "User", "globalStorage");
                    if (!string.IsNullOrEmpty(cursorPathOverride) && File.Exists(cursorPathOverride))
                    {
                        AddInstance(instances, "Cursor", cursorPathOverride, cursorStoragePath, VisualStudioCodeInstallationType.User, VisualStudioCodeType.Cursor);
                    }
                    else
                    {
                        var cursorPath = Path.Combine(appDataBasePath, "Cursor", "Cursor.exe");
                        if (File.Exists(cursorPath))
                        {
                            AddInstance(instances, "Cursor", cursorPath, cursorStoragePath, VisualStudioCodeInstallationType.User, VisualStudioCodeType.Cursor);
                        }
                    }
                }
                if (enabledEditions.HasFlag(VisualStudioCodeEdition.Antigravity))
                {
                     var antigravityStoragePath = Path.Combine(appDataBasePath, "Antigravity", "User", "globalStorage");
                     if (!string.IsNullOrEmpty(antigravityPathOverride) && File.Exists(antigravityPathOverride))
                    {
                        AddInstance(instances, "Antigravity", antigravityPathOverride, antigravityStoragePath, VisualStudioCodeInstallationType.User, VisualStudioCodeType.Antigravity);
                    }
                    else
                    {
                        var antigravityPath = Path.Combine(appdataProgramFilesPath, "Programs", "antigravity", "Antigravity.exe");
                        AddInstance(instances, "Antigravity", antigravityPath, antigravityStoragePath, VisualStudioCodeInstallationType.User, VisualStudioCodeType.Antigravity);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        private static void AddInstance(List<VisualStudioCodeInstance> instances, string name, string path, string storagePath, VisualStudioCodeInstallationType type, VisualStudioCodeType codeType)
        {
            try
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
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }
    }
}