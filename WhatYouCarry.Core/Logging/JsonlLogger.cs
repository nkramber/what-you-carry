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
    public static string BuildLine(LogLevel level, string message, LogFields fields)
    {
        StringBuilder builder = new();
        builder.Append('{');
        AppendText(builder, "level", LevelName(level));
        builder.Append(',');
        AppendText(builder, "message", message);

        foreach (LogField field in fields.Fields)
        {
            builder.Append(',');
            if (field.Quoted)
            {
                AppendText(builder, field.Name, field.Value);
            }
            else
            {
                AppendRaw(builder, field.Name, field.Value);
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

    /// <summary>Appends one name and one quoted value, with both escaped.</summary>
    private static void AppendText(StringBuilder builder, string name, string value)
    {
        AppendQuoted(builder, name);
        builder.Append(':');
        AppendQuoted(builder, value);
    }

    /// <summary>Appends one name and one value that the caller already formed, such as a number or a list.</summary>
    private static void AppendRaw(StringBuilder builder, string name, string value)
    {
        AppendQuoted(builder, name);
        builder.Append(':');
        builder.Append(value);
    }

    /// <summary>
    /// Appends one JSON string. It escapes the two characters that JSON reserves, and every control character
    /// below the space, so one line always stays one line.
    /// </summary>
    private static void AppendQuoted(StringBuilder builder, string value)
    {
        builder.Append('"');
        foreach (char letter in value)
        {
            switch (letter)
            {
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                case '\n': builder.Append("\\n"); break;
                case '\r': builder.Append("\\r"); break;
                case '\t': builder.Append("\\t"); break;
                case '\b': builder.Append("\\b"); break;
                case '\f': builder.Append("\\f"); break;
                default:
                    if (letter < ' ')
                    {
                        builder.Append("\\u00");
                        builder.Append(HexDigit(letter >> 4));
                        builder.Append(HexDigit(letter & 0xF));
                    }
                    else
                    {
                        builder.Append(letter);
                    }

                    break;
            }
        }

        builder.Append('"');
    }

    /// <summary>One lowercase hexadecimal digit for a value from 0 to 15.</summary>
    private static char HexDigit(int value)
    {
        return value < 10 ? (char)('0' + value) : (char)('a' + (value - 10));
    }
}
