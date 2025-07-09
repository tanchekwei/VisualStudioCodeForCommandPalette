using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WorkspaceLauncherForVSCode.Classes
{
    [JsonSerializable(typeof(List<VisualStudioCodeWorkspace>))]
    internal sealed partial class VisualStudioCodeWorkspaceSerializerContext : JsonSerializerContext
    {
    }
}
