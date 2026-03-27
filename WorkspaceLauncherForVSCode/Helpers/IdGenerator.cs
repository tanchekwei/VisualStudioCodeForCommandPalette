// Modifications copyright (c) 2025 tanchekwei
// Licensed under the MIT License. See the LICENSE file in the project root for details.
using System;
using System.Globalization;
using WyHash;

namespace WorkspaceLauncherForVSCode.Helpers
{
    public static class IdGenerator
    {
        private static readonly IFormatProvider Invariant = CultureInfo.InvariantCulture;
        public static string GetVisualStudioId(VisualStudioCodeWorkspace workspace)
        {
            var hash = ComputeHash(
                workspace.VSInstance?.InstancePath,
                workspace.WorkspaceTypeString,
                workspace.WindowsPath
            );
            return hash.ToString("X16", Invariant);
        }

        public static string GetVisualStudioCodeId(VisualStudioCodeWorkspace workspace)
        {
            var hash = ComputeHash(
                workspace.VSCodeInstance?.ExecutablePath,
                workspace.WorkspaceTypeString,
                workspace.WindowsPath
            );
            return hash.ToString("X16", Invariant);
        }

        private static ulong ComputeHash(params string?[] parts)
        {
            var result = ConcatParts(parts);
            return WyHash64.ComputeHash64(result.ToString(), seed: 0);
        }

        private static string ConcatParts(params string?[] parts)
        {
            const char sep = '\u001F';
            int length = 0;
            for (int i = 0; i < parts.Length; i++)
                length += (parts[i]?.Length ?? 0) + (i > 0 ? 1 : 0);

            return string.Create(length, parts, (span, p) =>
            {
                int pos = 0;
                for (int i = 0; i < p.Length; i++)
                {
                    if (i > 0) span[pos++] = sep;
                    if (!string.IsNullOrEmpty(p[i]))
                    {
                        p[i].AsSpan().CopyTo(span[pos..]);
                        pos += (p[i] ?? "").Length;
                    }
                }
            });
        }
    }
}
