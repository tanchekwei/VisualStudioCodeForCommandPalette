// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models.Json;

namespace WorkspaceLauncherForVSCode.Services.VisualStudio.Models
{
    public class VisualStudioInstance
    {
        private const string CodeContainersJson = "CodeContainers.json";

        public string Name => $"Visual Studio {ProductLineVersion}";

        public string ProductLineVersion { get; }

        public bool IsPrerelease { get; }

        public string ApplicationPrivateSettingsPath { get; }

        public string InstancePath { get; }

        public VisualStudioInstance(Json.VisualStudioInstance visualStudioInstance, string applicationPrivateSettingsPath)
        {
            try
            {
                ProductLineVersion = visualStudioInstance.Catalog.ProductLineVersion;
                IsPrerelease = visualStudioInstance.IsPrerelease;
                ApplicationPrivateSettingsPath = applicationPrivateSettingsPath;
                InstancePath = visualStudioInstance.ProductPath;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                ProductLineVersion = string.Empty;
                ApplicationPrivateSettingsPath = string.Empty;
                InstancePath = string.Empty;
            }
        }

        public IEnumerable<CodeContainer> GetCodeContainers()
        {
            var codeContainersString = GetCodeContainersPath();
            if (string.IsNullOrWhiteSpace(codeContainersString))
            {
                return [];
            }

            try
            {
                var containers = JsonSerializer.Deserialize(codeContainersString, CodeContainerSerializerContext.Default.ListCodeContainer);

                if (containers == null)
                {
                    return [];
                }

                var results = new List<CodeContainer>(containers.Count);
                foreach (var c in containers)
                {
                    results.Add(new CodeContainer(c, this));
                }
                return results;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return [];
            }
        }

        private string? GetCodeContainersPath()
        {
            try
            {
                if (ApplicationPrivateSettingsPath == null || !File.Exists(ApplicationPrivateSettingsPath))
                {
                    return null;
                }

                using var reader = XmlReader.Create(ApplicationPrivateSettingsPath, new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                    DtdProcessing = DtdProcessing.Prohibit,
                });

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "collection")
                    {
                        if (reader.GetAttribute("name") == "CodeContainers.Offline")
                        {
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "value")
                                {
                                    return reader.ReadElementContentAsString();
                                }

                                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "collection")
                                {
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                return null;
            }
        }
    }
}