using System.Collections.Generic;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;
using WorkspaceLauncherForVSCode.Interfaces;

namespace WorkspaceLauncherForVSCode.Services
{
    public class VisualStudioCodeInstanceProviderWrapper : IVisualStudioCodeInstanceProvider
    {
        public List<VisualStudioCodeInstance> GetInstances(VisualStudioCodeEdition enabledEditions, string preferredEdition)
        {
            return VisualStudioCodeInstanceProvider.GetInstances(enabledEditions, preferredEdition);
        }
    }
}