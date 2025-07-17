// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WorkspaceLauncherForVSCode.Commands;

public static class PageCommandResultHandler
{
    public static CommandResult HandleCommandResult(VisualStudioCodePage? page)
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
}
