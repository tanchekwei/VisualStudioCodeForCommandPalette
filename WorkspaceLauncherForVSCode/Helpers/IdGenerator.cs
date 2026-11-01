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

        private static ulong ComputeHash(string? p1, string? p2, string? p3)
        {
            const char sep = '\u001F';
            int len1 = p1?.Length ?? 0;
            int len2 = p2?.Length ?? 0;
            int len3 = p3?.Length ?? 0;
            int totalLength = len1 + 1 + len2 + 1 + len3;

            var result = string.Create(totalLength, (p1, p2, p3), (span, state) =>
            {
                int pos = 0;
                if (!string.IsNullOrEmpty(state.p1))
                {
                    state.p1.AsSpan().CopyTo(span[pos..]);
                    pos += state.p1.Length;
                }
                span[pos++] = sep;
                if (!string.IsNullOrEmpty(state.p2))
                {
                    state.p2.AsSpan().CopyTo(span[pos..]);
                    pos += state.p2.Length;
                }
                span[pos++] = sep;
                if (!string.IsNullOrEmpty(state.p3))
                {
                    state.p3.AsSpan().CopyTo(span[pos..]);
                }
            });

            return WyHash64.ComputeHash64(result, seed: 0);
        }

        private static ulong ComputeHash(params string?[] parts)
        {
            var result = ConcatParts(parts);
            return WyHash64.ComputeHash64(result, seed: 0);
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