using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Assets;

/// <summary>
/// The shape checks of a JSON value in an asset file. Each one names the file, the owner, and the field in its
/// error, so a bad file reports the box, the bone, or the keyframe at fault and never a bare parse error
/// (D-92, T-2).
/// </summary>
/// <remarks>
/// A Blockbench file holds many editor fields that the game never reads, such as the colors and the open state
/// of the outliner. The model loader reads a named subset and passes over the rest, so a real file loads. The
/// unknown-field check of D-168 does not apply to a file that another program writes (OQ-159). The animation
/// loader writes its own format, so it applies the check.
/// </remarks>
public static class JsonShape
{
    /// <summary>The count of components of a vector field.</summary>
    public const int VectorLength = 3;

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

    /// <summary>One whole number member, as an int.</summary>
    /// <exception cref="ContextException">The member is absent, or it is not a whole number that an int holds.</exception>
    public static int WholeNumber(string path, JsonElement owner, string ownerName, string name)
    {
        JsonElement value = Member(path, owner, ownerName, name);
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int number))
        {
            throw ContentError.Make(path, name, $"on '{ownerName}' is not a whole number that an int holds");
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

    /// <summary>A list member of a fixed count of numbers.</summary>
    /// <exception cref="ContextException">The list is absent, or it is not a list of that many numbers.</exception>
    public static float[] Numbers(string path, JsonElement list, string ownerName, string name, int count)
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

    /// <summary>Every member name of an object must stand in the permitted list (D-168).</summary>
    /// <exception cref="ContextException">The object holds a member that the list does not name.</exception>
    public static void CheckNoUnknownMember(string path, JsonElement owner, string ownerName, string[] permitted)
    {
        foreach (JsonProperty property in owner.EnumerateObject())
        {
            if (System.Array.IndexOf(permitted, property.Name) < 0)
            {
                throw ContentError.Make(path, property.Name, $"on '{ownerName}' is not a field of the format (D-168)");
            }
        }
    }
}
