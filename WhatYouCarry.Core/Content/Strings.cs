using System.Collections.Generic;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// The string table (D-98). Every string that a player sees has an id here, and no code holds one inline (G-8).
/// </summary>
/// <remarks>
/// English only in v1. The table is one content file, `strings/en.json`, and each name of that object is an id.
/// An unknown id is an error, because a screen with a blank line hides the defect (T-2).
/// </remarks>
public sealed class Strings
{
    /// <summary>The content path of the one string file of v1.</summary>
    public const string FilePath = "strings/en.json";

    private readonly List<JsonMember> entries;

    private Strings(List<JsonMember> entries)
    {
        this.entries = entries;
    }

    /// <summary>The count of ids in the table.</summary>
    public int Count => this.entries.Count;

    /// <summary>The table of one string file. Every value must be text.</summary>
    /// <exception cref="ContextException">A value is not text.</exception>
    public static Strings FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        List<JsonMember> entries = [];
        foreach (JsonMember member in members)
        {
            if (member.Kind != JsonMemberKind.Text)
            {
                throw ContentError.Make(path, member.Name, "holds a value that is not text, and every string is text");
            }

            entries.Add(member);
        }

        return new Strings(entries);
    }

    /// <summary>The text of one id.</summary>
    /// <exception cref="ContextException">The table holds no such id.</exception>
    public string Get(string id)
    {
        foreach (JsonMember entry in this.entries)
        {
            if (entry.Name == id)
            {
                return entry.Value;
            }
        }

        ContextException error = new($"The string table holds no id '{id}'.");
        error.AddContext("id", id);
        error.AddContext("file", FilePath);
        throw error;
    }

    /// <summary>Answers whether the table holds one id.</summary>
    public bool Has(string id)
    {
        foreach (JsonMember entry in this.entries)
        {
            if (entry.Name == id)
            {
                return true;
            }
        }

        return false;
    }
}
