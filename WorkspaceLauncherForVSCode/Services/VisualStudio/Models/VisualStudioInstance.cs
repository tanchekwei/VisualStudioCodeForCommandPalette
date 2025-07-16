// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml;
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
            ProductLineVersion = visualStudioInstance.Catalog.ProductLineVersion;
            IsPrerelease = visualStudioInstance.IsPrerelease;
            ApplicationPrivateSettingsPath = applicationPrivateSettingsPath;
            InstancePath = visualStudioInstance.ProductPath;
        }

        public IEnumerable<CodeContainer> GetCodeContainers()
        {
            var codeContainersString = GetCodeContainersPath();
            if (string.IsNullOrWhiteSpace(codeContainersString))
            {
                return Enumerable.Empty<CodeContainer>();
            }

            try
            {
                var containers = JsonSerializer.Deserialize(codeContainersString, CodeContainerSerializerContext.Default.ListCodeContainer);

                return containers == null
                    ? Enumerable.Empty<CodeContainer>()
                    : containers.Select(c => new CodeContainer(c, this));
            }
            catch (Exception)
            {
                return Enumerable.Empty<CodeContainer>();
            }
        }

        private string? GetCodeContainersPath()
        {
            try
            {
                if (ApplicationPrivateSettingsPath == null)
                {
                    return null;
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(ApplicationPrivateSettingsPath);

                using var collectionNodes = xmlDoc.GetElementsByTagName("collection");
                var collectionName = "CodeContainers.Offline";
                var collectionNode = null as XmlNode;

                foreach (XmlNode node in collectionNodes)
                {
                    var nameAttribute = node.Attributes?["name"];
                    if (nameAttribute != null && nameAttribute.Value == collectionName)
                    {
                        collectionNode = node;
                        break;
                    }
                }

                if (collectionNode != null)
                {
                    var valueNode = collectionNode?.SelectSingleNode("value");
                    return valueNode?.InnerText;
                }

                return null;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}