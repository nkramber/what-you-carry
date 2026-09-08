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
        ["System.Enum"] = new("L-REFLECTION", "An Enum method reads the enum metadata at run time. Compare the value, or hold an explicit list (G-2)."),
        ["System.Attribute"] = new("L-REFLECTION", "An Attribute method reads the metadata at run time. Core is static (G-2)."),
        ["System.AppDomain"] = new("L-REFLECTION", "AppDomain reads the loaded assemblies at run time. Core is static (G-2)."),
        ["System.Delegate"] = new("L-REFLECTION", "A Delegate member reads or makes a method at run time. Core is static (G-2)."),
        ["System.MulticastDelegate"] = new("L-REFLECTION", "A Delegate member reads or makes a method at run time. Core is static (G-2)."),
        ["System.RuntimeTypeHandle"] = new("L-REFLECTION", "A runtime handle names a type at run time. Core is static (G-2)."),
        ["System.RuntimeMethodHandle"] = new("L-REFLECTION", "A runtime handle names a method at run time. Core is static (G-2)."),
        ["System.RuntimeFieldHandle"] = new("L-REFLECTION", "A runtime handle names a field at run time. Core is static (G-2)."),
        ["System.Runtime.CompilerServices.RuntimeHelpers"] = new("L-REFLECTION", "RuntimeHelpers reads the object identity and the type at run time. Core is static (G-2)."),
        ["System.ComponentModel.TypeDescriptor"] = new("L-REFLECTION", "TypeDescriptor reads the type metadata at run time, beside reflection. Core is static (G-2)."),
        ["System.Guid"] = new("L-RANDOM", "A new Guid draws from the platform, not from the run seed. The seed is the one source of randomness (G-21)."),
        ["System.HashCode"] = new("L-IDENTITY", "HashCode takes a new seed in each process, so its result changes between runs (G-21)."),
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

    /// <summary>
    /// The reason that the scan gives for a <c>typeof</c> expression. It gives back a <c>System.Type</c>, which
    /// is the entry to every reflection call, and the value can reach a place that names no banned type (F-64).
    /// </summary>
    public const string TypeOfDetail = "typeof gives back System.Type, which reads the type at run time. Core is static (G-2).";

    /// <summary>
    /// A member that Core must not call, although its type is approved. The key is the full member name.
    /// </summary>
    /// <remarks>
    /// The default hash of a reference type is its address, and the string hash takes a new seed in each
    /// process. Either one changes an iteration order between two runs of one seed (G-21, F-67).
    /// </remarks>
    public static readonly IReadOnlyDictionary<string, BannedName> Members = new Dictionary<string, BannedName>
    {
        ["System.Object.GetHashCode"] = new("L-IDENTITY", "The default hash of a reference type is its address, and it changes between runs (G-21)."),
        ["System.String.GetHashCode"] = new("L-IDENTITY", "The string hash takes a new seed in each process, so it changes between runs (G-21)."),
    };

    /// <summary>
    /// The namespaces that Core may use (D-205). Every other namespace is a finding, so a metadata surface that
    /// no denylist names, such as `System.ComponentModel` or `System.Linq.Expressions`, cannot reach Core.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each entry matches one namespace and never its children. `System` does not allow `System.ComponentModel`.
    /// A later Core PR that needs another namespace adds it here, with a decision that says why (G-16, F-66).
    /// </para>
    /// <para>
    /// A denylist over the whole class library cannot be complete. The PR #12 review reopened the reflection
    /// finding four times, and each pass named one more type. This list turns the boundary around.
    /// </para>
    /// </remarks>
    public static readonly IReadOnlySet<string> AllowedNamespaces = new HashSet<string>
    {
        "System.Collections.Generic",
        "System.Globalization",
        "System.Numerics",
        "System.Runtime.CompilerServices",
    };

    /// <summary>The one namespace that Core approves by type and not as a whole.</summary>
    public const string SystemNamespace = "System";

    /// <summary>
    /// The `System` types that Core may use (D-206). `System` is broad, and it holds the rest of the
    /// nondeterminism beside the denied types: Guid, HashCode, GC, OperatingSystem, Console, and AppContext.
    /// Core approves its `System` surface one type at a time (F-67).
    /// </summary>
    /// <remarks>
    /// A primitive keyword such as `float` names no symbol on its own, so this list holds a primitive only when
    /// Core calls one of its members, as `float.IsNegative` does. A later Core PR adds a type here with a
    /// decision that says why (G-16).
    /// </remarks>
    public static readonly IReadOnlySet<string> AllowedSystemTypes = new HashSet<string>
    {
        "ArgumentOutOfRangeException",
        "BitConverter",
        "IEquatable",
        "Int32",
        "InvalidOperationException",
        "Object",
        "Single",
        "String",
        "UInt64",
    };

    /// <summary>The reason that the scan gives for a `System` type outside <see cref="AllowedSystemTypes"/>.</summary>
    public const string SystemTypeDetail = "Core approves its System types one at a time, because System holds the platform randomness, time, and machine state (G-21, G-16, D-206).";

    /// <summary>The prefix of this project's own namespaces. Core may use any namespace under it.</summary>
    public const string ProjectNamespacePrefix = "WhatYouCarry.";

    /// <summary>The reason that the scan gives for a namespace outside <see cref="AllowedNamespaces"/>.</summary>
    public const string NamespaceDetail = "Core may use only an approved namespace, so a metadata or platform surface cannot enter without a decision (G-2, G-16, D-205).";

    /// <summary>
    /// The reason that the scan gives for a conditional compilation directive (D-204, F-65). A `#if` makes two
    /// programs from one file, and a lint that parses one set of symbols can never read the other. Core holds
    /// one simulation, so it holds no conditional branch (D-69).
    /// </summary>
    public const string ConditionalDetail = "Conditional compilation makes two programs from one Core file, and only one of them reaches this check. Core is one simulation (D-69, D-204).";

    /// <summary>The rule id and the reason for one banned symbol.</summary>
    public readonly record struct BannedName(string Rule, string Detail);
}
