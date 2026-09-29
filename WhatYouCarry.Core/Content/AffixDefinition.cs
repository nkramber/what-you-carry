using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Content;

/// <summary>One named whole-number parameter of an affix, as the file holds it.</summary>
public readonly record struct AffixParameter(string Name, long Value);

/// <summary>
/// One affix definition (D-49, D-168, D-747): the id, the name of one coded behavior, and the parameters of that
/// behavior. <see cref="Items.AffixBehaviors"/> holds the code of each behavior.
/// </summary>
/// <remarks>
/// <para>
/// The behavior decides the parameter names, so the validator reads the behavior first and then checks the names of
/// that behavior alone. A behavior that Core does not have fails with its name (D-752).
/// </para>
/// <para>
/// Every parameter is a whole number, as in the other types (D-266): a percent, a damage, or a radius in
/// centimeters.
/// </para>
/// </remarks>
/// <param name="Id">The affix id, unique among the affixes.</param>
/// <param name="Behavior">One of <see cref="Lifesteal"/>, <see cref="Burning"/>, and <see cref="Swift"/>.</param>
/// <param name="Parameters">The parameters of the behavior, in the order of <see cref="ParameterNames"/>.</param>
public sealed record AffixDefinition(string Id, string Behavior, IReadOnlyList<AffixParameter> Parameters)
{
    /// <summary>Heals the wielder by a percent of the damage of a hit that lands (D-747).</summary>
    public const string Lifesteal = "lifesteal";

    /// <summary>Deals damage to each foe near the struck target (D-747).</summary>
    public const string Burning = "burning";

    /// <summary>Makes the melee windup of the wielded weapon shorter by a percent (D-747).</summary>
    public const string Swift = "swift";

    /// <summary>The percent of lifesteal and of swift.</summary>
    public const string PercentName = "percent";

    /// <summary>The damage of burning.</summary>
    public const string DamageName = "damage";

    /// <summary>The radius of burning around the feet center of the struck target, in centimeters (D-751).</summary>
    public const string RadiusName = "radiusCentimetres";

    /// <summary>The names that every affix carries before the parameters of its behavior.</summary>
    public static readonly IReadOnlyList<string> SharedNames =
    [
        "id",
        "behavior",
    ];

    /// <summary>An affix carries no optional name.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>The coded behaviors, in a fixed order (D-49, D-747).</summary>
    public static readonly IReadOnlyList<string> Behaviors =
    [
        Lifesteal,
        Burning,
        Swift,
    ];

    /// <summary>The parameter names of one behavior.</summary>
    /// <exception cref="ContextException">Core has no behavior of the name.</exception>
    public static IReadOnlyList<string> ParameterNames(string behavior)
    {
        if (behavior == Lifesteal || behavior == Swift)
        {
            return [PercentName];
        }

        if (behavior == Burning)
        {
            return [DamageName, RadiusName];
        }

        ContextException error = new($"Core has no affix behavior '{behavior}' (D-49, D-747).");
        error.AddContext("behavior", behavior);
        throw error;
    }

    /// <summary>The value of one parameter.</summary>
    /// <exception cref="ContextException">The affix holds no parameter of the name, which a validated affix of this behavior always holds.</exception>
    public long Parameter(string name)
    {
        foreach (AffixParameter parameter in this.Parameters)
        {
            if (parameter.Name == name)
            {
                return parameter.Value;
            }
        }

        ContextException error = new($"The affix '{this.Id}' of the behavior '{this.Behavior}' holds no parameter '{name}'.");
        error.AddContext("affix", this.Id);
        error.AddContext("behavior", this.Behavior);
        error.AddContext("parameter", name);
        throw error;
    }

    /// <summary>One definition from one validated object.</summary>
    /// <exception cref="ContextException">A field is absent, unknown, of another kind, or outside its bounds, or the behavior is not one of <see cref="Behaviors"/>.</exception>
    public static AffixDefinition FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        string behavior = ContentValidator.Value(path, members, "behavior", JsonMemberKind.Text);
        bool known = false;
        foreach (string name in Behaviors)
        {
            if (name == behavior)
            {
                known = true;
            }
        }

        if (!known)
        {
            throw ContentError.Make(path, "behavior", $"names '{behavior}', and Core has no affix behavior of that name (D-49, D-752)");
        }

        IReadOnlyList<string> parameterNames = ParameterNames(behavior);
        List<string> required = [.. SharedNames];
        foreach (string name in parameterNames)
        {
            required.Add(name);
        }

        ContentValidator.Check(path, members, required, Optional);

        List<AffixParameter> parameters = [];
        foreach (string name in parameterNames)
        {
            parameters.Add(new AffixParameter(name, Bounded(path, members, name)));
        }

        return new AffixDefinition(ContentValidator.Value(path, members, "id", JsonMemberKind.Text), behavior, parameters);
    }

    /// <summary>One parameter inside its bounds: a percent from 1 to 100, and a damage or a radius of one or more.</summary>
    private static long Bounded(string path, IReadOnlyList<JsonMember> members, string name)
    {
        string text = ContentValidator.Value(path, members, name, JsonMemberKind.Number);
        if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
        {
            throw ContentError.Make(path, name, "holds a number that does not fit a whole number");
        }

        if (name == PercentName && (value < 1 || value > 100))
        {
            throw ContentError.Make(path, name, $"is {value}, and a percent is from 1 to 100");
        }

        if (value < 1)
        {
            throw ContentError.Make(path, name, $"is {value}, and the parameter is one or more");
        }

        return value;
    }
}
