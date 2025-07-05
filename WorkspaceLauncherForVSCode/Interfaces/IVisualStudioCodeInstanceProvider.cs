using System.Collections.Generic;
using WorkspaceLauncherForVSCode.Classes;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Interfaces
{
    public interface IVisualStudioCodeInstanceProvider
    {
        List<VisualStudioCodeInstance> GetInstances(VisualStudioCodeEdition enabledEditions, string preferredEdition);
    }
}