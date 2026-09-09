using System.Collections.Generic;
using System.Text.Json;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// Reads one JSON object into named text values (D-91, D-220). Each validator then reads the names it needs and
/// rejects any name that it does not know (D-92, D-168).
/// </summary>
/// <remarks>
/// <para>
/// The reader is `Utf8JsonReader`, a forward-only reader over bytes. It uses no serializer and no reflection, so
/// nothing reads a type at run time and G-2 holds.
/// </para>
/// <para>
/// Every value arrives as text. A validator turns it into a number or a list, because the validator owns the
/// shape of its type and the reader owns nothing but the names.
/// </para>
/// </remarks>
public static class JsonObjectReader
{
    /// <summary>The named values of one JSON object, in file order.</summary>
    /// <exception cref="Logging.ContextException">The bytes are not one JSON object, or a name repeats.</exception>
    public static IReadOnlyList<JsonMember> Read(string path, IReadOnlyList<byte> bytes)
    {
        byte[] input = new byte[bytes.Count];
        for (int index = 0; index < bytes.Count; index++)
        {
            input[index] = bytes[index];
        }

        List<JsonMember> members = [];

        // The reader raises its own error type, and that error names no file. Every content failure names the
        // file, the field, and the reason, so this method turns one into the other (D-92, T-2).
        try
        {
        Utf8JsonReader reader = new(input, new JsonReaderOptions { CommentHandling = JsonCommentHandling.Disallow });

        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw ContentError.MakeForFile(path, "the file must hold one JSON object");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (reader.Read())
                {
                    throw ContentError.MakeForFile(path, "the file holds more than one JSON value");
                }

                return members;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw ContentError.MakeForFile(path, "the file holds a value outside a name");
            }

            string name = reader.GetString() ?? string.Empty;
            foreach (JsonMember member in members)
            {
                if (member.Name == name)
                {
                    throw ContentError.Make(path, name, "appears twice, and a JSON object with two equal names has no defined reading");
                }
            }

            if (!reader.Read())
            {
                throw ContentError.Make(path, name, "has no value");
            }

            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    members.Add(new JsonMember(name, reader.GetString() ?? string.Empty, JsonMemberKind.Text));
                    break;
                case JsonTokenType.Number:
                    members.Add(new JsonMember(name, reader.GetInt64().ToString(System.Globalization.CultureInfo.InvariantCulture), JsonMemberKind.Number));
                    break;
                case JsonTokenType.True:
                    members.Add(new JsonMember(name, "true", JsonMemberKind.Truth));
                    break;
                case JsonTokenType.False:
                    members.Add(new JsonMember(name, "false", JsonMemberKind.Truth));
                    break;
                default:
                    throw ContentError.Make(path, name, "holds a value that no Phase 1 content type uses");
            }
        }

        throw ContentError.MakeForFile(path, "the file ends before the object closes");
        }
        catch (JsonException error)
        {
            throw ContentError.MakeForFile(path, $"the file is not valid JSON. {error.Message}");
        }
    }
}

/// <summary>One name and value of a JSON object. The value is text, and the kind says what the file held.</summary>
public readonly record struct JsonMember(string Name, string Value, JsonMemberKind Kind);

/// <summary>What a JSON file held for one name.</summary>
public enum JsonMemberKind
{
    /// <summary>A JSON string.</summary>
    Text = 0,

    /// <summary>A JSON number, which Phase 1 reads as a whole number.</summary>
    Number = 1,

    /// <summary>A JSON true or false.</summary>
    Truth = 2,
}
