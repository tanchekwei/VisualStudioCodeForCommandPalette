// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.IO;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Services.VisualStudio.Models
{
    public class CodeContainer
    {
        public string Name { get; private set; }

        public string FullPath { get; private set; }

        public bool IsFavorite { get; private set; }

        public DateTime LastAccessed { get; private set; }

        public VisualStudioInstance Instance { get; private set; }

        public CodeContainer(Json.CodeContainer codeContainer, VisualStudioInstance instance)
        {
            try
            {
                Name = Path.GetFileName(codeContainer.Value.LocalProperties.FullPath);
                FullPath = codeContainer.Value.LocalProperties.FullPath;
                IsFavorite = codeContainer.Value.IsFavorite;
                LastAccessed = codeContainer.Value.LastAccessed;
                Instance = instance;
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex);
                Name = string.Empty;
                FullPath = string.Empty;
                Instance = instance;
            }
        }
    }
}
