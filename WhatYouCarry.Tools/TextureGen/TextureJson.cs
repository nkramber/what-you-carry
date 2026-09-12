using System.Text.Json;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>The JSON parse of a palette file or a rule file. A file that does not parse is an error that names the file (T-2).</summary>
public static class TextureJson
{
    /// <summary>The name of the root object in an error.</summary>
    public const string RootName = "the file";

    /// <summary>The document of one file, whose root is an object. The caller disposes it.</summary>
    /// <exception cref="ContextException">The bytes are not valid JSON, or the root is not an object.</exception>
    public static JsonDocument Parse(string path, byte[] bytes)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(bytes);
        }
        catch (JsonException error)
        {
            throw ContentError.MakeForFile(path, $"the file is not valid JSON. {error.Message}");
        }

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            document.Dispose();
            throw ContentError.MakeForFile(path, "the file must hold one JSON object");
        }

        return document;
    }
}
