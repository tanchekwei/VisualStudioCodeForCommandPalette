using System.Collections.Generic;
using System.Text.Json.Serialization;
using WorkspaceLauncherForVSCode.Services.VisualStudio.Models;

namespace WorkspaceLauncherForVSCode.Classes
{
    [JsonSerializable(typeof(List<VisualStudioCodeWorkspace>))]
    [JsonSerializable(typeof(VisualStudioCodeInstance), TypeInfoPropertyName = "VSCodeInstance")]
    [JsonSerializable(typeof(VisualStudioInstance), TypeInfoPropertyName = "VSInstance")]
    internal sealed partial class VisualStudioCodeWorkspaceSerializerContext : JsonSerializerContext
    {
    }
}
