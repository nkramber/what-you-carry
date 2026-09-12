using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WhatYouCarry.Assets;

namespace WhatYouCarry.Tools.AssetQa;

/// <summary>
/// The file case check (D-135, D-302): every file name reference in a content file, a model file, or an
/// animation file names a file that exists, with the same case as the directory entry.
/// </summary>
/// <remarks>
/// <para>
/// A reference is a string value, at any depth of the JSON, that ends in one of the reference extensions. It
/// resolves relative to the content directory. The check compares each segment of the path with the entries
/// of its directory by ordinal comparison, and never asks the file system whether the file exists, because a
/// case-insensitive file system says yes to the wrong case. Linux says no, and the game then fails there and
/// nowhere else (D-191).
/// </para>
/// <para>
/// A reference with a backslash, a rooted path, or a dot segment is a finding too, because it resolves on one
/// machine and not on another.
/// </para>
/// </remarks>
public static class FileCaseCheck
{
    /// <summary>The extensions that mark a string value as a file name reference (D-302).</summary>
    public static readonly IReadOnlyList<string> ReferenceExtensions = [".json", ".bbmodel", ".png"];

    /// <summary>The file patterns that the check reads for references.</summary>
    private static readonly string[] Patterns = ["*.json", "*.bbmodel"];

    /// <summary>Every case finding under one content directory, in ordinal path order of the referencing file.</summary>
    public static IReadOnlyList<AssetFinding> Run(string contentRoot)
    {
        List<AssetFinding> findings = [];
        foreach (string file in SortedFiles(contentRoot))
        {
            string path = AssetSet.ContentPath(contentRoot, file);
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(File.ReadAllBytes(file));
            }
            catch (JsonException error)
            {
                findings.Add(new AssetFinding(path, $"the file is not valid JSON. {error.Message}"));
                continue;
            }

            using (document)
            {
                List<string> references = [];
                CollectReferences(document.RootElement, references);
                foreach (string reference in references)
                {
                    string? problem = Resolve(contentRoot, reference);
                    if (problem is not null)
                    {
                        findings.Add(new AssetFinding(path, $"references '{reference}', and {problem} (D-302)"));
                    }
                }
            }
        }

        return findings;
    }

    /// <summary>Answers whether a string value is a file name reference: it ends in a reference extension, in any case.</summary>
    public static bool IsReference(string value)
    {
        foreach (string extension in ReferenceExtensions)
        {
            if (value.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Every JSON file and model file under the root, at any depth, in ordinal order.</summary>
    private static string[] SortedFiles(string contentRoot)
    {
        List<string> files = [];
        foreach (string pattern in Patterns)
        {
            files.AddRange(Directory.GetFiles(contentRoot, pattern, SearchOption.AllDirectories));
        }

        string[] sorted = [.. files];
        Array.Sort(sorted, StringComparer.Ordinal);
        return sorted;
    }

    /// <summary>Every string value under one element that is a reference, in document order.</summary>
    private static void CollectReferences(JsonElement element, List<string> references)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                string value = element.GetString() ?? string.Empty;
                if (IsReference(value))
                {
                    references.Add(value);
                }

                break;
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    CollectReferences(property.Value, references);
                }

                break;
            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    CollectReferences(item, references);
                }

                break;
            default:
                break;
        }
    }

    /// <summary>
    /// The problem with one reference, or null when a file of that exact path exists. The walk reads each
    /// directory listing and matches one segment at a time by ordinal comparison.
    /// </summary>
    private static string? Resolve(string contentRoot, string reference)
    {
        if (reference.IndexOf('\\', StringComparison.Ordinal) >= 0)
        {
            return "a reference uses forward slashes";
        }

        if (reference.StartsWith('/') || Path.IsPathRooted(reference))
        {
            return "a reference is relative to the content directory and never rooted";
        }

        string[] segments = reference.Split('/');
        string directory = contentRoot;
        for (int index = 0; index < segments.Length; index++)
        {
            string segment = segments[index];
            if (segment.Length == 0 || segment == "." || segment == "..")
            {
                return "a reference has no empty segment and no dot segment";
            }

            bool last = index == segments.Length - 1;
            string? match = MatchEntry(directory, segment, last, out string? caseMatch);
            if (match is null)
            {
                if (caseMatch is not null)
                {
                    return $"the entry there is '{caseMatch}', which differs in case, so the reference fails on a case-sensitive file system";
                }

                string kind = last ? "no file" : "no directory";
                return $"{kind} '{segment}' exists in '{AssetSet.ContentPath(contentRoot, directory)}'";
            }

            directory = Path.Combine(directory, match);
        }

        return null;
    }

    /// <summary>
    /// The entry of a directory that equals the segment by ordinal comparison, or null. When none equals it and
    /// one equals it in another case, that one comes back as the case match.
    /// </summary>
    private static string? MatchEntry(string directory, string segment, bool file, out string? caseMatch)
    {
        caseMatch = null;
        if (!Directory.Exists(directory))
        {
            return null;
        }

        string[] entries = file ? Directory.GetFiles(directory) : Directory.GetDirectories(directory);
        foreach (string entry in entries)
        {
            string name = Path.GetFileName(entry);
            if (name == segment)
            {
                return name;
            }

            if (string.Equals(name, segment, StringComparison.OrdinalIgnoreCase))
            {
                caseMatch = name;
            }
        }

        return null;
    }
}
