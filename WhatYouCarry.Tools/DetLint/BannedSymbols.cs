using System.Collections.Generic;

namespace WhatYouCarry.Tools.DetLint;

/// <summary>
/// The symbols that Core must not use, and why (G-2, G-21, D-67). This class holds the rules alone. The scan
/// that applies them is <see cref="CoreSourceScan"/>.
/// </summary>
/// <remarks>
/// Every key here is the full name of a symbol that the compiler resolves, never a word in the source text.
/// The word `Math` can name `System.Math`, or a Core type that a later PR declares, and only the compiler knows
/// which one. The scan asks the compiler first and reads this table after (F-64).
/// </remarks>
public static class BannedSymbols
{
    /// <summary>
    /// The one Core file that may use <c>MathF</c>, and only for the members in <see cref="AllowedMathFMembers"/>.
    /// The rule reads the whole path and never the file name alone. A second file with the same name in another
    /// directory takes no exemption (F-64).
    /// </summary>
    public const string DetMathPath = "WhatYouCarry.Core/Determinism/DetMath.cs";

    /// <summary>The full name of the one type that <see cref="DetMathPath"/> may use.</summary>
    public const string MathFType = "System.MathF";

    /// <summary>
    /// The <c>MathF</c> members that <c>DetMath.cs</c> may call. Each one is an exact IEEE operation, so it gives
    /// the same bits on every platform. A transcendental such as <c>MathF.Sin</c> is never on this list, because
    /// the platform library, and not this repository, decides its last bits (D-69).
    /// </summary>
    /// <remarks>A new member joins this list with a test that shows the member is exact.</remarks>
    public static readonly IReadOnlySet<string> AllowedMathFMembers = new HashSet<string>
    {
        "Sqrt",
        "Abs",
        "Floor",
    };

    /// <summary>
    /// A type that Core must not use, by its full name, with the rule id and the reason. The scan reports a use
    /// of the type itself and a use of any member of it.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, BannedName> Types = new Dictionary<string, BannedName>
    {
        ["System.Math"] = new("L-MATH", "The platform decides the last bits of a Math function. Use DetMath (G-2)."),
        [MathFType] = new("L-MATHF", "Only one Core file may use MathF, and only for an exact IEEE operation (G-2)."),
        ["System.Random"] = new("L-RANDOM", "The seed is the one source of randomness. Use Rng (G-21)."),
        ["System.DateTime"] = new("L-CLOCK", "The tick is the one source of time. A wall clock is not deterministic (G-21)."),
        ["System.DateTimeOffset"] = new("L-CLOCK", "The tick is the one source of time. A wall clock is not deterministic (G-21)."),
        ["System.TimeProvider"] = new("L-CLOCK", "The tick is the one source of time. A wall clock is not deterministic (G-21)."),
        ["System.Diagnostics.Stopwatch"] = new("L-CLOCK", "The tick is the one source of time. A stopwatch is not deterministic (G-21)."),
        ["System.Environment"] = new("L-CLOCK", "Environment reads the machine, and TickCount is a wall clock (G-21)."),
        ["System.Type"] = new("L-REFLECTION", "System.Type reads the type at run time. Core is static (G-2)."),
        ["System.Activator"] = new("L-REFLECTION", "Activator makes a type at run time. Core is static (G-2)."),
        ["System.Numerics.Vector"] = new("L-SIMD", "A hardware vector gives another result width on another platform (G-2)."),
        ["System.Numerics.Vector2"] = new("L-SIMD", "A System.Numerics vector may use SIMD. Core declares its own types (G-2)."),
        ["System.Numerics.Vector3"] = new("L-SIMD", "A System.Numerics vector may use SIMD. Core declares its own types (G-2)."),
        ["System.Numerics.Vector4"] = new("L-SIMD", "A System.Numerics vector may use SIMD. Core declares its own types (G-2)."),
    };

    /// <summary>
    /// A namespace that Core must not use. The scan reports a type in the namespace, a member of such a type,
    /// and an import of the namespace.
    /// </summary>
    /// <remarks>
    /// The list holds only the two namespaces that have no correct use in Core. It does not hold System.Numerics
    /// or System.Diagnostics, because each of those holds a deterministic type as well, such as BitOperations.
    /// The banned types of those two are in <see cref="Types"/> by their own full names.
    /// </remarks>
    public static readonly IReadOnlyDictionary<string, BannedName> Namespaces = new Dictionary<string, BannedName>
    {
        ["System.Reflection"] = new("L-REFLECTION", "Reflection reads the assembly at run time. Core is static (G-2)."),
        ["System.Runtime.Intrinsics"] = new("L-SIMD", "An intrinsic is hardware dependent (G-2)."),
    };

    /// <summary>The rule id and the reason for one banned symbol.</summary>
    public readonly record struct BannedName(string Rule, string Detail);
}
