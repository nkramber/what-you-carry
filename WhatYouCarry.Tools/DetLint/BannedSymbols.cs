using System.Collections.Generic;

namespace WhatYouCarry.Tools.DetLint;

/// <summary>
/// The symbols that Core must not name, and why (G-2, G-21, D-67). This class holds the rules alone. The scan
/// that applies them is <see cref="CoreSourceScan"/>.
/// </summary>
public static class BannedSymbols
{
    /// <summary>The one Core file that may name <c>MathF</c>, and only for the members in <see cref="AllowedMathFMembers"/>.</summary>
    public const string DetMathFileName = "DetMath.cs";

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
    /// A type name that Core must not name, with the rule id and the reason. The scan reads the identifier, so a
    /// name inside a comment or a string is never a finding.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, BannedName> Names = new Dictionary<string, BannedName>
    {
        ["Math"] = new("L-MATH", "The platform decides the last bits of a Math function. Use DetMath (G-2)."),
        ["MathF"] = new("L-MATHF", $"Only {DetMathFileName} may name MathF, and only for an exact IEEE operation (G-2)."),
        ["Random"] = new("L-RANDOM", "The seed is the one source of randomness. Use Rng (G-21)."),
        ["DateTime"] = new("L-CLOCK", "The tick is the one source of time. A wall clock is not deterministic (G-21)."),
        ["DateTimeOffset"] = new("L-CLOCK", "The tick is the one source of time. A wall clock is not deterministic (G-21)."),
        ["Stopwatch"] = new("L-CLOCK", "The tick is the one source of time. A stopwatch is not deterministic (G-21)."),
        ["Environment"] = new("L-CLOCK", "Environment reads the machine, and TickCount is a wall clock (G-21)."),
        ["Vector"] = new("L-SIMD", "A hardware vector gives another result width on another platform (G-2)."),
        ["Vector2"] = new("L-SIMD", "A System.Numerics vector may use SIMD. Core declares its own types (G-2)."),
        ["Vector3"] = new("L-SIMD", "A System.Numerics vector may use SIMD. Core declares its own types (G-2)."),
        ["Vector4"] = new("L-SIMD", "A System.Numerics vector may use SIMD. Core declares its own types (G-2)."),
        ["Vector64"] = new("L-SIMD", "An intrinsic vector is hardware dependent (G-2)."),
        ["Vector128"] = new("L-SIMD", "An intrinsic vector is hardware dependent (G-2)."),
        ["Vector256"] = new("L-SIMD", "An intrinsic vector is hardware dependent (G-2)."),
        ["Vector512"] = new("L-SIMD", "An intrinsic vector is hardware dependent (G-2)."),
        ["dynamic"] = new("L-DYNAMIC", "Dynamic dispatch hides the call that runs. Core is explicit (G-2, T-1)."),
    };

    /// <summary>
    /// A namespace that Core must not import or name. A using directive of one of these, or a qualified name that
    /// starts with one, is a finding.
    /// </summary>
    /// <remarks>
    /// The list holds only the two namespaces that have no correct use in Core. It does not hold System.Numerics
    /// or System.Diagnostics, because each of those holds a deterministic type as well, such as BitOperations.
    /// The banned members of those two are in <see cref="Names"/> by their own type names.
    /// </remarks>
    public static readonly IReadOnlyDictionary<string, BannedName> Namespaces = new Dictionary<string, BannedName>
    {
        ["System.Reflection"] = new("L-REFLECTION", "Reflection reads the assembly at run time. Core is static (G-2)."),
        ["System.Runtime.Intrinsics"] = new("L-SIMD", "An intrinsic is hardware dependent (G-2)."),
    };

    /// <summary>The rule id and the reason for one banned name.</summary>
    public readonly record struct BannedName(string Rule, string Detail);
}
