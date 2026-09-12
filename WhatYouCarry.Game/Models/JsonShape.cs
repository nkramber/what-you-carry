using System.Globalization;
using System.Text.Json;
using Godot;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Models;

/// <summary>
/// The shape checks of a JSON value in a model file. Each one names the file, the owner, and the field in its
/// error, so a bad model reports the box or the bone at fault and never a bare parse error (D-92, T-2).
/// </summary>
/// <remarks>
/// A Blockbench file holds many editor fields that the game never reads, such as the colors and the open state
/// of the outliner. The loader reads a named subset and passes over the rest, so a real file loads. The
/// unknown-field check of D-168 does not apply to a file that another program writes (OQ-159).
/// </remarks>
public static class JsonShape
{
    /// <summary>The count of components of a vector field.</summary>
    public const int VectorLength = 3;

    /// <summary>The count of numbers of a face rectangle: two corners.</summary>
    public const int RectangleLength = 4;

    /// <summary>One member of an object. An absent member is an error that names the owner.</summary>
    /// <exception cref="ContextException">The value is not an object, or it has no such member.</exception>
    public static JsonElement Member(string path, JsonElement owner, string ownerName, string name)
    {
        if (owner.ValueKind != JsonValueKind.Object)
        {
            throw ContentError.Make(path, name, $"needs an object on '{ownerName}', and the value there is not one");
        }

        if (!owner.TryGetProperty(name, out JsonElement value))
        {
            throw ContentError.Make(path, name, $"is absent on '{ownerName}'");
        }

        return value;
    }

    /// <summary>One text member.</summary>
    /// <exception cref="ContextException">The member is absent, or it is not text.</exception>
    public static string Text(string path, JsonElement owner, string ownerName, string name)
    {
        JsonElement value = Member(path, owner, ownerName, name);
        if (value.ValueKind != JsonValueKind.String)
        {
            throw ContentError.Make(path, name, $"on '{ownerName}' is not text");
        }

        return value.GetString() ?? string.Empty;
    }

    /// <summary>One number member, as a float.</summary>
    /// <exception cref="ContextException">The member is absent, or it is not a number that a float holds.</exception>
    public static float Number(string path, JsonElement owner, string ownerName, string name)
    {
        JsonElement value = Member(path, owner, ownerName, name);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetSingle(out float number) || !float.IsFinite(number))
        {
            throw ContentError.Make(path, name, $"on '{ownerName}' is not a number that a float holds");
        }

        return number;
    }

    /// <summary>One vector member: a list of three numbers.</summary>
    /// <exception cref="ContextException">The member is absent, or it is not a list of three numbers.</exception>
    public static Vector3 Vector(string path, JsonElement owner, string ownerName, string name)
    {
        float[] numbers = Numbers(path, Member(path, owner, ownerName, name), ownerName, name, VectorLength);
        return new Vector3(numbers[0], numbers[1], numbers[2]);
    }

    /// <summary>One rectangle member: a list of four numbers, two corners.</summary>
    /// <exception cref="ContextException">The member is absent, or it is not a list of four numbers.</exception>
    public static Vector4 Rectangle(string path, JsonElement owner, string ownerName, string name)
    {
        float[] numbers = Numbers(path, Member(path, owner, ownerName, name), ownerName, name, RectangleLength);
        return new Vector4(numbers[0], numbers[1], numbers[2], numbers[3]);
    }

    /// <summary>A list of a fixed count of numbers.</summary>
    private static float[] Numbers(string path, JsonElement list, string ownerName, string name, int count)
    {
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() != count)
        {
            throw ContentError.Make(path, name, $"on '{ownerName}' is not a list of {count.ToString(CultureInfo.InvariantCulture)} numbers");
        }

        float[] numbers = new float[count];
        int index = 0;
        foreach (JsonElement item in list.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Number || !item.TryGetSingle(out float number) || !float.IsFinite(number))
            {
                throw ContentError.Make(path, name, $"on '{ownerName}' holds a value that is not a number that a float holds, at index {index.ToString(CultureInfo.InvariantCulture)}");
            }

            numbers[index] = number;
            index++;
        }

        return numbers;
    }
}
