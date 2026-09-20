using System.Collections.Generic;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// The shared checks of every content validator (D-92, D-168). Each type declares its required names, and the
/// checks reject an absent one and an unknown one.
/// </summary>
/// <remarks>
/// One hand-written validator per type, and no schema file. The types are few, and each list is explicit, so a
/// reader sees the whole shape of a type in one place (D-168).
/// </remarks>
public static class ContentValidator
{
    /// <summary>
    /// Checks one object against a required-name list. Every required name must appear, and every name that
    /// appears must stand in the required list or the optional list.
    /// </summary>
    /// <exception cref="ContextException">A required name is absent, or an unknown name appears.</exception>
    public static void Check(string path, IReadOnlyList<JsonMember> members, IReadOnlyList<string> required, IReadOnlyList<string> optional)
    {
        foreach (string name in required)
        {
            bool found = false;
            foreach (JsonMember member in members)
            {
                if (member.Name == name)
                {
                    found = true;
                    break;
                }
            }

            // An absent field is an error, and never a zero (D-92, T-2).
            if (!found)
            {
                throw ContentError.Make(path, name, "is absent, and this type requires it");
            }
        }

        foreach (JsonMember member in members)
        {
            bool known = false;
            foreach (string name in required)
            {
                if (member.Name == name)
                {
                    known = true;
                    break;
                }
            }

            foreach (string name in optional)
            {
                if (member.Name == name)
                {
                    known = true;
                    break;
                }
            }

            // An unknown field means the file and the code disagree about the type, and a silent skip hides it.
            if (!known)
            {
                throw ContentError.Make(path, member.Name, "is not a field of this type");
            }
        }
    }

    /// <summary>The value of one name, which the caller already knows is present.</summary>
    /// <exception cref="ContextException">The name is absent, or the file held another kind of value.</exception>
    public static string Value(string path, IReadOnlyList<JsonMember> members, string name, JsonMemberKind kind)
    {
        foreach (JsonMember member in members)
        {
            if (member.Name != name)
            {
                continue;
            }

            if (member.Kind != kind)
            {
                throw ContentError.Make(path, name, $"holds a {KindName(member.Kind)} value, and this type needs a {KindName(kind)} value");
            }

            return member.Value;
        }

        throw ContentError.Make(path, name, "is absent, and this type requires it");
    }

    /// <summary>
    /// The whole numbers of a list value, which the reader gives as the item texts with a comma between them. The
    /// scan reads the text letter by letter, because Core holds no string member that splits one (G-21).
    /// </summary>
    /// <exception cref="ContextException">The text holds a letter that is no digit, no minus sign, and no comma, or a number of more than four digits.</exception>
    public static IReadOnlyList<long> Numbers(string path, string name, string text)
    {
        List<long> numbers = [];
        long value = 0;
        bool digits = false;
        bool negative = false;
        for (int index = 0; index <= text.Length; index++)
        {
            char letter = index < text.Length ? text[index] : ',';
            if (letter == ',')
            {
                if (!digits)
                {
                    throw ContentError.Make(path, name, "holds a list item with no digit");
                }

                numbers.Add(negative ? -value : value);
                value = 0;
                digits = false;
                negative = false;
                continue;
            }

            if (letter == '-' && !digits && !negative)
            {
                negative = true;
                continue;
            }

            if (letter < '0' || letter > '9')
            {
                throw ContentError.Make(path, name, $"holds the letter '{letter}' in a list of whole numbers");
            }

            value = (value * 10) + (letter - '0');
            digits = true;
            if (value > LargestListNumber)
            {
                throw ContentError.Make(path, name, $"holds a list item over {LargestListNumber}, and no field of a list takes one");
            }
        }

        return numbers;
    }

    /// <summary>The largest whole number that a list value takes. No field of a list needs a larger one.</summary>
    public const long LargestListNumber = 9999;

    /// <summary>The name of one kind in a message. The switch is explicit, so no reflection reads the enum (G-2).</summary>
    private static string KindName(JsonMemberKind kind)
    {
        switch (kind)
        {
            case JsonMemberKind.Text: return "text";
            case JsonMemberKind.Number: return "number";
            case JsonMemberKind.Truth: return "true or false";
            case JsonMemberKind.EmptyList: return "empty list";
            case JsonMemberKind.Null: return "null";
            case JsonMemberKind.NumberList: return "list of numbers";
            default: return "unknown";
        }
    }
}
