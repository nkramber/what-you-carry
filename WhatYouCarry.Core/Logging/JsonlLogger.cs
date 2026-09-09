using System.Collections.Generic;
using System.Text;

namespace WhatYouCarry.Core.Logging;

/// <summary>
/// The logger. It writes one JSON object per line to a sink, and it rejects a line that lacks a required field
/// of its context (D-68, D-113, D-211, D-212).
/// </summary>
/// <remarks>
/// <para>
/// Every line carries a level and a message beside its context fields. No line carries a wall-clock time,
/// because the tick is the only time in Core, and a run line already names its tick (G-21, D-212).
/// </para>
/// <para>
/// The logger writes no file. It hands the finished line to an <see cref="ILogSink"/>, so a disk failure never
/// reaches the simulation thread (D-211, D-72).
/// </para>
/// </remarks>
public sealed class JsonlLogger
{
    /// <summary>The fields that a line inside a run must carry (D-113).</summary>
    public static readonly IReadOnlyList<string> RequiredRunFields =
    [
        "seed",
        "floor",
        "tick",
        "subsystem",
        "entities",
    ];

    /// <summary>The fields that a line outside a run must carry (D-113).</summary>
    public static readonly IReadOnlyList<string> RequiredHubFields =
    [
        "saveVersions",
        "screen",
        "action",
        "paths",
    ];

    private readonly ILogSink sink;

    /// <summary>A logger that writes to one sink.</summary>
    public JsonlLogger(ILogSink sink)
    {
        this.sink = sink;
    }

    /// <summary>The required field names of one context (D-113).</summary>
    /// <exception cref="ContextException">The context is not a declared value.</exception>
    public static IReadOnlyList<string> RequiredFields(LogContextKind context)
    {
        // An explicit bound, because Enum.IsDefined reads the enum through reflection, and Core has none (G-2).
        switch (context)
        {
            case LogContextKind.Run: return RequiredRunFields;
            case LogContextKind.Hub: return RequiredHubFields;
            default: throw new ContextException($"The log context must be a value of LogContextKind. The value is {(int)context}.");
        }
    }

    /// <summary>
    /// Writes one line. The line holds the level, the message, and every field that the caller added, in that
    /// order.
    /// </summary>
    /// <exception cref="ContextException">A required field of the context is absent, or the message is empty.</exception>
    public void Write(LogContextKind context, LogLevel level, string message, LogFields fields)
    {
        if (message.Length == 0)
        {
            throw new ContextException($"A log line needs a message, and this {ContextName(context)} line has none.");
        }

        // An absent required field is an error, and never a line with a hole in it (T-2, D-113).
        foreach (string required in RequiredFields(context))
        {
            if (!fields.Has(required))
            {
                ContextException error = new($"A {ContextName(context)} log line must carry the field '{required}', and this one does not.");
                error.AddContext("context", ContextName(context));
                error.AddContext("missingField", required);
                error.AddContext("message", message);
                throw error;
            }
        }

        this.sink.Write(BuildLine(level, message, fields));
    }

    /// <summary>One JSON object, with no line break. The level and the message come first, then every field.</summary>
    /// <remarks>
    /// This method owns the line. It calls one helper for one job: <c>LevelName</c> for the severity name, and
    /// <c>AppendQuoted</c> for each JSON string. Neither helper calls another one, so the whole write path stays
    /// one level deep (D-110, F-73).
    /// </remarks>
    public static string BuildLine(LogLevel level, string message, LogFields fields)
    {
        StringBuilder builder = new();
        builder.Append('{');

        AppendQuoted(builder, LogFields.LevelName);
        builder.Append(':');
        AppendQuoted(builder, LevelName(level));

        builder.Append(',');
        AppendQuoted(builder, LogFields.MessageName);
        builder.Append(':');
        AppendQuoted(builder, message);

        foreach (LogField field in fields.Fields)
        {
            builder.Append(',');
            AppendQuoted(builder, field.Name);
            builder.Append(':');

            if (field.Quoted)
            {
                AppendQuoted(builder, field.Value);
            }
            else
            {
                // The caller already formed this value as a number, a list, or a keyword.
                builder.Append(field.Value);
            }
        }

        builder.Append('}');
        return builder.ToString();
    }

    /// <summary>The name that one context takes in a message. The switch is explicit, so no reflection reads the enum (G-2).</summary>
    private static string ContextName(LogContextKind context)
    {
        switch (context)
        {
            case LogContextKind.Run: return "run";
            case LogContextKind.Hub: return "hub";
            default: return "unknown";
        }
    }

    /// <summary>The name that one level takes in a line. The switch is explicit, so no reflection reads the enum (G-2).</summary>
    private static string LevelName(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Debug: return "debug";
            case LogLevel.Info: return "info";
            case LogLevel.Warning: return "warning";
            case LogLevel.Error: return "error";
            default: throw new ContextException($"The log level must be a value of LogLevel. The value is {(int)level}.");
        }
    }

    /// <summary>
    /// Appends one JSON string. It escapes the two characters that JSON reserves, every control character below
    /// the space, and every surrogate that stands without its pair.
    /// </summary>
    /// <remarks>
    /// An unpaired surrogate is not text that UTF-8 can hold. A raw one makes the line unparseable, and the
    /// `\u` escape form leaves a value that a reader cannot take back, so this method writes the replacement
    /// character in its place (F-73). This method calls no helper.
    /// </remarks>
    private static void AppendQuoted(StringBuilder builder, string value)
    {
        builder.Append('"');
        for (int index = 0; index < value.Length; index++)
        {
            char letter = value[index];
            bool isHighSurrogate = letter >= '\uD800' && letter <= '\uDBFF';
            bool isLowSurrogate = letter >= '\uDC00' && letter <= '\uDFFF';

            // A high surrogate with its low partner is one character, and the pair stays as it is.
            if (isHighSurrogate && index + 1 < value.Length && value[index + 1] >= '\uDC00' && value[index + 1] <= '\uDFFF')
            {
                builder.Append(letter);
                builder.Append(value[index + 1]);
                index++;
                continue;
            }

            switch (letter)
            {
                case '"': builder.Append("\\\""); continue;
                case '\\': builder.Append("\\\\"); continue;
                case '\n': builder.Append("\\n"); continue;
                case '\r': builder.Append("\\r"); continue;
                case '\t': builder.Append("\\t"); continue;
                case '\b': builder.Append("\\b"); continue;
                case '\f': builder.Append("\\f"); continue;
                default: break;
            }

            // A surrogate without its pair is not valid text, and no JSON escape gives it back: a reader that
            // asks for the string still fails on it. The replacement character is the standard mark for text
            // that was not valid, and it keeps the whole value readable (F-73).
            if (isHighSurrogate || isLowSurrogate)
            {
                builder.Append('\uFFFD');
                continue;
            }

            if (letter >= ' ')
            {
                builder.Append(letter);
                continue;
            }

            // A control character takes four lowercase hexadecimal digits, most significant first.
            builder.Append("\\u");
            for (int shift = 12; shift >= 0; shift -= 4)
            {
                int digit = (letter >> shift) & 0xF;
                builder.Append((char)(digit < 10 ? '0' + digit : 'a' + (digit - 10)));
            }
        }

        builder.Append('"');
    }
}
