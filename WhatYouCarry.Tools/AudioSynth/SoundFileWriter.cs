using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WhatYouCarry.Tools.AudioSynth;

/// <summary>
/// Writes a sound parameter file: indented JSON, with a list of numbers on one line. A frame of 64 levels then takes one
/// line, so a person can read the file and a diff stays short.
/// </summary>
public static class SoundFileWriter
{
    private static readonly JsonWriterOptions Indented = new() { Indented = true };

    /// <summary>The UTF-8 bytes of one JSON value, with a line end at the end.</summary>
    public static byte[] Write(JsonNode root)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream, Indented))
        {
            WriteNode(writer, root);
        }

        stream.WriteByte((byte)'\n');
        return stream.ToArray();
    }

    /// <summary>Writes one value: an object or a list member by member, a list of numbers on one line, and any other value as it is.</summary>
    private static void WriteNode(Utf8JsonWriter writer, JsonNode? node)
    {
        switch (node)
        {
            case JsonObject owner:
                writer.WriteStartObject();
                foreach (var property in owner)
                {
                    writer.WritePropertyName(property.Key);
                    WriteNode(writer, property.Value);
                }

                writer.WriteEndObject();
                break;
            case JsonArray list when list.Count > 0 && list.All(item => item is JsonValue value && value.GetValueKind() == JsonValueKind.Number):
                writer.WriteRawValue("[" + string.Join(",", list.Select(item => item!.ToJsonString())) + "]");
                break;
            case JsonArray list:
                writer.WriteStartArray();
                foreach (JsonNode? item in list)
                {
                    WriteNode(writer, item);
                }

                writer.WriteEndArray();
                break;
            case null:
                writer.WriteNullValue();
                break;
            default:
                node.WriteTo(writer);
                break;
        }
    }
}
