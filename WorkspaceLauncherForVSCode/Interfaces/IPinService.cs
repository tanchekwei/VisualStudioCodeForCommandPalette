// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System.Threading.Tasks;

namespace WorkspaceLauncherForVSCode.Interfaces
{
    public interface IPinService
    {
        Task TogglePinStatusAsync(string path);
    }
}