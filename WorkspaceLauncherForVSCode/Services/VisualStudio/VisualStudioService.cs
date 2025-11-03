// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models.Json;

using VsCodeModels = WorkspaceLauncherForVSCode.Services.VisualStudio.Models;

namespace WorkspaceLauncherForVSCode.Services.VisualStudio
{
    public class VisualStudioService
    {
        private const string VsWhereDir = @"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer";
        private const string VsWhereBin = "vswhere.exe";
        private const string VisualStudioDataDir = @"%LOCALAPPDATA%\Microsoft\VisualStudio";
        private const int SingleDirectory = 0;

        private List<VsCodeModels.VisualStudioInstance>? _instances;

        public ReadOnlyCollection<VsCodeModels.VisualStudioInstance>? Instances => _instances?.AsReadOnly();

        public VisualStudioService()
        {
        }

        public void InitInstances(string[] excludedVersions)
        {
            try
            {
#if DEBUG
                using var logger = new TimeLogger();
#endif
                if (_instances != null)
                {
                    return;
                }
                var paths = new string?[] { null, VsWhereDir };
                var exceptions = new List<(string? Path, Exception Exception)>(paths.Length);
                _instances = new();

                foreach (var path in paths)
                {
                    try
                    {
                        var vsWherePath = path != null
                            ? Path.Combine(path, VsWhereBin)
                            : VsWhereBin;

                        vsWherePath = Environment.ExpandEnvironmentVariables(vsWherePath);

                        if (!File.Exists(vsWherePath))
                        {
                            throw new FileNotFoundException($"vswhere.exe not found at path: {vsWherePath}");
                        }

                        var startInfo = new ProcessStartInfo(vsWherePath, "-all -prerelease -format json")
                        {
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                        };

                        using var process = Process.Start(startInfo);
                        if (process == null)
                        {
                            continue;
                        }

                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit(TimeSpan.FromSeconds(5));
                        if (string.IsNullOrWhiteSpace(output))
                        {
                            continue;
                        }

                        var instancesJson = JsonSerializer.Deserialize(output, VisualStudioInstanceSerializerContext.Default.ListVisualStudioInstance);
                        if (instancesJson == null)
                        {
                            continue;
                        }

                        foreach (var instance in instancesJson)
                        {
                            var applicationPrivateSettingsPath = GetApplicationPrivateSettingsPathByInstanceId(instance.InstanceId);
                            if (string.IsNullOrWhiteSpace(applicationPrivateSettingsPath))
                            {
                                continue;
                            }

                            bool isExcluded = false;
                            foreach (var excluded in excludedVersions)
                            {
                                if (excluded == instance.Catalog.ProductLineVersion)
                                {
                                    isExcluded = true;
                                    break;
                                }
                            }
                            if (isExcluded)
                            {
                                continue;
                            }

                            _instances.Add(new VsCodeModels.VisualStudioInstance(instance, applicationPrivateSettingsPath));
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add((path, ex));
                    }
                }

                if (_instances?.Count == 0)
                {
                    foreach (var ex in exceptions)
                    {
                        ErrorLogger.LogError(ex.Exception);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }
        }

        public IEnumerable<VsCodeModels.CodeContainer> GetResults(bool showPrerelease)
        {
            try
            {
                if (_instances == null)
                {
                    return new List<VsCodeModels.CodeContainer>();
                }

                var filteredInstances = new List<VsCodeModels.VisualStudioInstance>();
                if (!showPrerelease)
                {
                    foreach (var instance in _instances)
                    {
                        if (!instance.IsPrerelease)
                        {
                            filteredInstances.Add(instance);
                        }
                    }
                }
                else
                {
                    filteredInstances.AddRange(_instances);
                }

                var results = new List<VsCodeModels.CodeContainer>();
                foreach (var instance in filteredInstances)
                {
                    results.AddRange(instance.GetCodeContainers());
                }

                results.Sort((a, b) =>
                {
                    int nameCompare = string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                    if (nameCompare != 0)
                    {
                        return nameCompare;
                    }
                    return a.Instance.IsPrerelease.CompareTo(b.Instance.IsPrerelease);
                });

                return results;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return new List<VsCodeModels.CodeContainer>();
            }
        }

        private static string? GetApplicationPrivateSettingsPathByInstanceId(string instanceId)
        {
            try
            {
                var dataPath = Environment.ExpandEnvironmentVariables(VisualStudioDataDir);
                var matchingDirs = new List<DirectoryInfo>();
                foreach (var dir in Directory.EnumerateDirectories(dataPath, $"*{instanceId}", SearchOption.TopDirectoryOnly))
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (!dirInfo.Name.StartsWith("SettingsBackup_", StringComparison.Ordinal))
                    {
                        matchingDirs.Add(dirInfo);
                    }
                }

                if (matchingDirs.Count == 1)
                {
                    var applicationPrivateSettingspath = Path.Combine(matchingDirs[SingleDirectory].FullName, "ApplicationPrivateSettings.xml");

                    if (File.Exists(applicationPrivateSettingspath))
                    {
                        return applicationPrivateSettingspath;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
            }

            return null;
        }
    }
}
