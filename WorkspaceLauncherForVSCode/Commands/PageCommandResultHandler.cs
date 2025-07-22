// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WorkspaceLauncherForVSCode.Classes;

namespace WorkspaceLauncherForVSCode.Commands;

public static class PageCommandResultHandler
{
    public static CommandResult HandleCommandResult(VisualStudioCodePage? page)
    {
        try
        {
            if (page != null && page.SettingsManager.ClearSearchOnExecute)
            {
                // reset search text
                if (!string.IsNullOrEmpty(page.SearchText))
                {
                    page.UpdateSearchText(page.SearchText, "");
                    page.SearchText = "";
                }
            }

            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
            return CommandResult.KeepOpen();
        }
    }
}
