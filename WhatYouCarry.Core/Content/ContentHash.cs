using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// The SHA-256 hash of a content set (D-163). The run record header carries it, and a replay compares it
/// against the set that the build holds (D-151).
/// </summary>
/// <remarks>
/// <para>
/// The hash reads each file's relative path and then its bytes, in sorted path order. The sort makes the answer
/// the same whatever order the host gives, and the path in the input makes a rename a change.
/// </para>
/// <para>
/// SHA-256 and not FNV-1a: this hash guards a run record against a content set that somebody changed, and
/// FNV-1a is fast and easy to collide on purpose. The state hash of D-160 needs the speed, and this one needs
/// the resistance (D-221).
/// </para>
/// </remarks>
public static class ContentHash
{
    /// <summary>The hash of one content set, as 64 lowercase hexadecimal digits.</summary>
    /// <exception cref="Logging.ContextException">Two files share one path.</exception>
    public static string Of(IReadOnlyList<ContentFile> files)
    {
        List<ContentFile> sorted = [];
        foreach (ContentFile file in files)
        {
            sorted.Add(file);
        }

        sorted.Sort(static (first, second) => string.CompareOrdinal(first.Path, second.Path));

        // A repeated path makes the order of two files matter, and the hash must not depend on it (T-2).
        for (int index = 1; index < sorted.Count; index++)
        {
            if (sorted[index].Path == sorted[index - 1].Path)
            {
                throw ContentError.MakeForFile(sorted[index].Path, "two content files share this path");
            }
        }

        List<byte> input = [];
        foreach (ContentFile file in sorted)
        {
            foreach (byte part in Encoding.UTF8.GetBytes(file.Path))
            {
                input.Add(part);
            }

            foreach (byte part in file.Bytes)
            {
                input.Add(part);
            }
        }

        StringBuilder text = new();
        foreach (byte part in SHA256.HashData(input.ToArray()))
        {
            text.Append((char)(part >> 4 < 10 ? '0' + (part >> 4) : 'a' + (part >> 4) - 10));
            text.Append((char)((part & 0xF) < 10 ? '0' + (part & 0xF) : 'a' + (part & 0xF) - 10));
        }

        return text.ToString();
    }
}
