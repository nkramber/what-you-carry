using System.Collections.Generic;
using System.Text;
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

        // The reader raises its own error type, and that error names no file. Every content failure names the
        // file, the field, and the reason, so this method turns one into the other (D-92, T-2).
        try
        {
        Utf8JsonReader reader = new(input, new JsonReaderOptions { CommentHandling = JsonCommentHandling.Disallow });

        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            throw ContentError.MakeForFile(path, "the file must hold one JSON object");
        }

        List<JsonMember> members = ReadMembers(path, ref reader, true);
        if (reader.Read())
        {
            throw ContentError.MakeForFile(path, "the file holds more than one JSON value");
        }

        return members;
        }
        catch (JsonException error)
        {
            throw ContentError.MakeForFile(path, $"the file is not valid JSON. {error.Message}");
        }
    }

    /// <summary>
    /// The named values of one object, from the token after its open brace to its close brace, in file order. The reader
    /// then stands on the close brace.
    /// </summary>
    /// <param name="outer">True for the object of the file, which can hold a list of objects. An object inside such a
    /// list holds no list of objects, so the shape stays one level deep (D-766).</param>
    /// <exception cref="Logging.ContextException">The file ends inside the object, a name repeats, or a value has a kind that no type uses.</exception>
    private static List<JsonMember> ReadMembers(string path, ref Utf8JsonReader reader, bool outer)
    {
        List<JsonMember> members = [];
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return members;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw ContentError.MakeForFile(path, "the file holds a value outside a name");
            }

            string name = Text(path, null, ref reader);
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
                    members.Add(new JsonMember(name, Text(path, name, ref reader), JsonMemberKind.Text));
                    break;
                case JsonTokenType.Number:
                    // The token text, and not a conversion. `GetInt64` throws its own error for a fractional or
                    // an out-of-range number, and that error names no file. The validator owns the number shape,
                    // and it reports the file and the field when the text does not fit (D-220, D-92, F-78).
                    members.Add(new JsonMember(name, Encoding.UTF8.GetString(reader.ValueSpan), JsonMemberKind.Number));
                    break;
                case JsonTokenType.True:
                    members.Add(new JsonMember(name, "true", JsonMemberKind.Truth));
                    break;
                case JsonTokenType.False:
                    members.Add(new JsonMember(name, "false", JsonMemberKind.Truth));
                    break;
                case JsonTokenType.StartArray:
                    // A list holds numbers, text, or objects, and one kind alone (D-229, D-346, D-766). A list of
                    // another kind of item is an error here.
                    members.Add(ReadList(path, name, ref reader, outer));
                    break;
                case JsonTokenType.Null:
                    members.Add(new JsonMember(name, string.Empty, JsonMemberKind.Null));
                    break;
                default:
                    throw ContentError.Make(path, name, "holds a value that no type uses");
            }
        }

        throw ContentError.MakeForFile(path, "the file ends before the object closes");
    }

    /// <summary>
    /// The text of the name or the string value under the reader. The reader checks UTF-8 only when it gives the text,
    /// and it then raises an error that names no file, so this method names the file and the field (T-2, F-149).
    /// </summary>
    /// <param name="field">The name that holds the value, or null when the text is a name.</param>
    /// <exception cref="Logging.ContextException">The text is not valid UTF-8.</exception>
    private static string Text(string path, string? field, ref Utf8JsonReader reader)
    {
        try
        {
            return reader.GetString() ?? string.Empty;
        }
        catch (System.InvalidOperationException)
        {
            const string Reason = "holds text that is not valid UTF-8";
            throw field is null
                ? ContentError.MakeForFile(path, "the file holds a name that is not valid UTF-8")
                : ContentError.Make(path, field, Reason);
        }
    }

    /// <summary>
    /// One list value, from the token after the open bracket. The first item sets the kind of the list. An empty list
    /// gives <see cref="JsonMemberKind.EmptyList"/>. A list of numbers gives <see cref="JsonMemberKind.NumberList"/>, and a
    /// list of text gives <see cref="JsonMemberKind.TextList"/>, each with the item texts, separated by commas. A list of
    /// objects gives <see cref="JsonMemberKind.ObjectList"/> with the members of each object (D-766).
    /// </summary>
    /// <param name="objectsAllowed">True when the list can hold objects: a list of the file object, and not of an object inside a list.</param>
    /// <exception cref="Logging.ContextException">The file ends inside the list, the items are of two kinds, an item is of a kind that no list holds, or a text item holds a comma.</exception>
    private static JsonMember ReadList(string path, string name, ref Utf8JsonReader reader, bool objectsAllowed)
    {
        StringBuilder items = new();
        List<IReadOnlyList<JsonMember>> objects = [];
        int count = 0;
        JsonMemberKind kind = JsonMemberKind.EmptyList;
        while (true)
        {
            if (!reader.Read())
            {
                throw ContentError.Make(path, name, "holds a list that the file ends inside");
            }

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return kind == JsonMemberKind.ObjectList
                    ? new JsonMember(name, string.Empty, kind, objects)
                    : new JsonMember(name, items.ToString(), kind);
            }

            JsonMemberKind itemKind = ListKindOf(path, name, reader.TokenType, objectsAllowed);
            if (count > 0 && itemKind != kind)
            {
                throw ContentError.Make(path, name, "holds a list with items of two kinds, and a list holds one kind of item");
            }

            kind = itemKind;
            if (kind == JsonMemberKind.ObjectList)
            {
                objects.Add(ReadMembers(path, ref reader, false));
                count++;
                continue;
            }

            if (count > 0)
            {
                items.Append(',');
            }

            if (kind == JsonMemberKind.NumberList)
            {
                items.Append(Encoding.UTF8.GetString(reader.ValueSpan));
            }
            else
            {
                string text = Text(path, name, ref reader);
                for (int index = 0; index < text.Length; index++)
                {
                    // The comma separates the items, so an item with a comma would read as two (T-2).
                    if (text[index] == ',')
                    {
                        throw ContentError.Make(path, name, $"holds the text item '{text}' with a comma, and a text item of a list holds no comma");
                    }
                }

                items.Append(text);
            }

            count++;
        }
    }

    /// <summary>The kind of a list whose item starts with this token.</summary>
    /// <exception cref="Logging.ContextException">The token starts an item that no list holds, or an object where no object fits.</exception>
    private static JsonMemberKind ListKindOf(string path, string name, JsonTokenType token, bool objectsAllowed)
    {
        if (token == JsonTokenType.Number)
        {
            return JsonMemberKind.NumberList;
        }

        if (token == JsonTokenType.String)
        {
            return JsonMemberKind.TextList;
        }

        if (token == JsonTokenType.StartObject && objectsAllowed)
        {
            return JsonMemberKind.ObjectList;
        }

        if (token == JsonTokenType.StartObject)
        {
            throw ContentError.Make(path, name, "holds a list of objects inside an object of a list, and the shape stays one level deep (D-766)");
        }

        throw ContentError.Make(path, name, "holds a list with an item that is not a number, a text, or an object");
    }
}

/// <summary>
/// One name and value of a JSON object. The value is text, and the kind says what the file held. A list of objects
/// holds its objects in <paramref name="Objects"/>, and every other kind holds null there.
/// </summary>
public readonly record struct JsonMember(string Name, string Value, JsonMemberKind Kind, IReadOnlyList<IReadOnlyList<JsonMember>>? Objects = null);

/// <summary>What a JSON file held for one name.</summary>
public enum JsonMemberKind
{
    /// <summary>A JSON string.</summary>
    Text = 0,

    /// <summary>A JSON number, which Phase 1 reads as a whole number.</summary>
    Number = 1,

    /// <summary>A JSON true or false.</summary>
    Truth = 2,

    /// <summary>A JSON list with no item. The run record header writes one for the loadout and the tree (D-229).</summary>
    EmptyList = 3,

    /// <summary>A JSON null. The run record header writes one for the amulet (D-229).</summary>
    Null = 4,

    /// <summary>A JSON list of numbers. The floor template reads one for its ramp slopes (D-346).</summary>
    NumberList = 5,

    /// <summary>A JSON list of text. A loadout entry of the run record header reads one for its affixes (D-766).</summary>
    TextList = 6,

    /// <summary>A JSON list of objects. The run record header reads one for its loadout (D-766).</summary>
    ObjectList = 7,
}
