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
        ["System.Object.GetType"] = new("L-REFLECTION", "GetType gives back System.Type, which reads the type at run time. Core is static (G-2)."),
        ["System.Object.GetHashCode"] = new("L-IDENTITY", "The default hash of a reference type is its address, and it changes between runs (G-21)."),
        ["System.String.GetHashCode"] = new("L-IDENTITY", "The string hash takes a new seed in each process, so it changes between runs (G-21)."),
        ["System.Globalization.CultureInfo.CurrentCulture"] = new("L-CLOCK", "The current culture reads the user, not the simulation input. Use InvariantCulture (G-21)."),
        ["System.Globalization.CultureInfo.CurrentUICulture"] = new("L-CLOCK", "The current culture reads the user, not the simulation input. Use InvariantCulture (G-21)."),
        ["System.Globalization.CultureInfo.InstalledUICulture"] = new("L-CLOCK", "The installed culture reads the machine, not the simulation input. Use InvariantCulture (G-21)."),
        ["System.Globalization.CultureInfo.DefaultThreadCurrentCulture"] = new("L-CLOCK", "The thread culture reads the process, not the simulation input. Use InvariantCulture (G-21)."),
        ["System.Globalization.CultureInfo.DefaultThreadCurrentUICulture"] = new("L-CLOCK", "The thread culture reads the process, not the simulation input. Use InvariantCulture (G-21)."),
    };

    /// <summary>
    /// Every type outside this project that Core may use, by full name (D-207). No namespace is approved as a
    /// whole, so a machine-dependent type cannot enter through a namespace that Core uses for something else.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A whole namespace is too coarse. `System.Collections.Generic` holds `EqualityComparer`, whose string hash
    /// takes a new seed in each process, and `System.Runtime.CompilerServices` holds `RuntimeFeature`, which
    /// reads the runtime. The PR #12 review found each of those behind an approved namespace (F-68).
    /// </para>
    /// <para>
    /// A primitive keyword such as `float` names no symbol on its own, so this list holds a primitive only when
    /// Core calls one of its members, as `float.IsNegative` does. The list also binds the names that Core writes
    /// and never the type that a method gives back, because every method that gives back a bool would need an
    /// entry. A later Core PR adds a type here with a decision that says why (G-16).
    /// </para>
    /// </remarks>
    public static readonly IReadOnlySet<string> AllowedTypes = new HashSet<string>
    {
        "System.ArgumentOutOfRangeException",
        "System.BitConverter",
        "System.IEquatable",
        "System.Int32",
        "System.InvalidOperationException",
        "System.Single",
        "System.String",
        "System.UInt64",
        "System.Globalization.CultureInfo",
        "System.Exception",
        "System.Int64",
        "System.Text.StringBuilder",
        "System.Collections.Generic.List",
        "System.Collections.Generic.IReadOnlyList",
        "System.Collections.Generic.IReadOnlyCollection",
        "System.Runtime.CompilerServices.CallerFilePathAttribute",
        "System.Runtime.CompilerServices.CallerLineNumberAttribute",
        "System.Runtime.CompilerServices.CallerMemberNameAttribute",
    };

    /// <summary>The prefix of this project's own namespaces. Core may use any type under it.</summary>
    public const string ProjectNamespacePrefix = "WhatYouCarry.";

    /// <summary>
    /// Every member of an approved type that Core may use (D-208). An approved type is not an approved surface:
    /// `String` holds `Intern`, which reads the process intern pool, and `CultureInfo` holds a constructor that
    /// reads the user settings. A member outside this list is a finding (F-69).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A method entry ends with a slash and the count of its parameters, because two overloads of one name do
    /// not share one behavior. `UInt64.ToString/2` takes a format provider and gives the same text on every
    /// machine, and `ToString/0` reads the current culture. A property and a field carry no count.
    /// </para>
    /// <para>
    /// A constructor uses the member name `new`. A member of a type of this project needs no entry.
    /// </para>
    /// </remarks>
    public static readonly IReadOnlySet<string> AllowedMembers = new HashSet<string>
    {
        "System.ArgumentOutOfRangeException.new/2",
        "System.BitConverter.SingleToUInt32Bits/1",
        "System.Globalization.CultureInfo.InvariantCulture",
        "System.Int32.MinValue",
        "System.InvalidOperationException.new/1",
        "System.Single.IsFinite/1",
        "System.Single.IsNegative/1",
        "System.UInt64.ToString/2",
        "System.Exception.new/1",
        "System.Exception.new/2",
        "System.Exception.Message",
        "System.Int64.ToString/1",
        "System.Single.ToString/2",
        "System.String.Length",
        "System.String.this[]",
        "System.Text.StringBuilder.new/0",
        "System.Text.StringBuilder.Append/1",
        "System.Text.StringBuilder.ToString/0",
        "System.Collections.Generic.List.Add/1",
        "System.Collections.Generic.List.Count",
        "System.Collections.Generic.List.this[]",
        "System.Collections.Generic.IReadOnlyList.this[]",
        "System.Collections.Generic.IReadOnlyCollection.Count",
        "System.Runtime.CompilerServices.CallerFilePathAttribute.new/0",
        "System.Runtime.CompilerServices.CallerLineNumberAttribute.new/0",
        "System.Runtime.CompilerServices.CallerMemberNameAttribute.new/0",
    };

    /// <summary>The reason that the scan gives for a member outside <see cref="AllowedMembers"/>.</summary>
    public const string MemberDetail = "Core approves each member it uses outside this project, because an approved type holds members that read the user, the process, or the machine (G-21, G-16, D-208).";

    /// <summary>The reason that the scan gives for a type outside <see cref="AllowedTypes"/>.</summary>
    public const string TypeDetail = "Core approves each type it uses outside this project, because a namespace holds machine-dependent types beside the ones Core needs (G-2, G-21, G-16, D-207).";

    /// <summary>The reason that the scan gives for an import that holds no approved type.</summary>
    public const string NamespaceDetail = "Core approves no type in this namespace, so the import can bring in nothing that Core may use (G-16, D-207).";


    /// <summary>
    /// The reason that the scan gives for a conditional compilation directive (D-204, F-65). A `#if` makes two
    /// programs from one file, and a lint that parses one set of symbols can never read the other. Core holds
    /// one simulation, so it holds no conditional branch (D-69).
    /// </summary>
    public const string ConditionalDetail = "Conditional compilation makes two programs from one Core file, and only one of them reaches this check. Core is one simulation (D-69, D-204).";

    /// <summary>The rule id and the reason for one banned symbol.</summary>
    public readonly record struct BannedName(string Rule, string Detail);
}
