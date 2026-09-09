using System.Collections.Generic;
using System.Globalization;

namespace WhatYouCarry.Core.Logging;

/// <summary>
/// The named values of one log line, in the order that the caller added them (D-68). The logger checks this set
/// against the required names of its context, and it writes every value that the caller added.
/// </summary>
/// <remarks>
/// The order is the order of the calls, so two runs of one seed give the same line. A repeated name is an error,
/// because a JSON object with two equal names has no defined reading (T-2).
/// </remarks>
public sealed class LogFields
{
    /// <summary>The name that the logger writes for the severity of a line (D-212). No caller field takes it.</summary>
    public const string LevelName = "level";

    /// <summary>The name that the logger writes for the text of a line (D-212). No caller field takes it.</summary>
    public const string MessageName = "message";

    /// <summary>The name that the assertion report writes for the file of the call site (D-215).</summary>
    public const string AssertFileName = "assertFile";

    /// <summary>The name that the assertion report writes for the line of the call site (D-215).</summary>
    public const string AssertLineName = "assertLine";

    /// <summary>The name that the assertion report writes for the member of the call site (D-215).</summary>
    public const string AssertMemberName = "assertMember";

    private readonly List<LogField> fields = [];

    /// <summary>Every field, in the order that the caller added it.</summary>
    public IReadOnlyList<LogField> Fields => this.fields;

    /// <summary>Adds a whole-number field, such as a seed, a floor, or a tick.</summary>
    public void Add(string name, long value)
    {
        this.AddField(name, value.ToString(CultureInfo.InvariantCulture), quoted: false);
    }

    /// <summary>Adds an unsigned whole-number field, such as the run seed of D-159.</summary>
    public void Add(string name, ulong value)
    {
        this.AddField(name, value.ToString(CultureInfo.InvariantCulture), quoted: false);
    }

    /// <summary>Adds a text field, such as a subsystem name or a file path.</summary>
    public void Add(string name, string value)
    {
        this.AddField(name, value, quoted: true);
    }

    /// <summary>Adds a true or false field.</summary>
    public void Add(string name, bool value)
    {
        this.AddField(name, value ? "true" : "false", quoted: false);
    }

    /// <summary>
    /// Adds a float field. The format keeps nine digits, which reads back as the same float, and the invariant
    /// culture keeps the separator the same on every machine (G-21).
    /// </summary>
    public void Add(string name, float value)
    {
        this.AddField(name, value.ToString("G9", CultureInfo.InvariantCulture), quoted: true);
    }

    /// <summary>Adds a list of whole numbers, such as the entity ids of a run line.</summary>
    public void Add(string name, IReadOnlyList<long> values)
    {
        System.Text.StringBuilder builder = new();
        builder.Append('[');
        for (int index = 0; index < values.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            builder.Append(values[index].ToString(CultureInfo.InvariantCulture));
        }

        builder.Append(']');
        this.AddField(name, builder.ToString(), quoted: false);
    }

    /// <summary>
    /// A copy of these fields, and the call site of an assertion after them (D-215). The caller keeps its own
    /// set unchanged (F-72).
    /// </summary>
    /// <remarks>
    /// The three call-site names are reserved, so no caller field carries one and this method never meets a
    /// repeated name. An assertion must always write its report, and a caller field must not stop it (F-74).
    /// </remarks>
    public LogFields CopyWithCallSite(string file, long line, string member)
    {
        LogFields copy = new();
        foreach (LogField field in this.fields)
        {
            copy.fields.Add(field);
        }

        copy.fields.Add(new LogField(AssertFileName, file, Quoted: true));
        copy.fields.Add(new LogField(AssertLineName, line.ToString(CultureInfo.InvariantCulture), Quoted: false));
        copy.fields.Add(new LogField(AssertMemberName, member, Quoted: true));
        return copy;
    }

    /// <summary>Answers whether a field of this name exists.</summary>
    public bool Has(string name)
    {
        foreach (LogField field in this.fields)
        {
            if (field.Name == name)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks one name and keeps the field. This method calls no other one, so the whole add path is one level
    /// below the caller (D-110).
    /// </summary>
    /// <remarks>
    /// A field name is an identifier that the code writes, and a name must reach the line unchanged. The writer
    /// puts the replacement character in place of a lone surrogate, so two names that differ only there would
    /// reach the object as one name, and a reader could then take either value. A name with a lone surrogate is
    /// a defect of the caller, and it is an error here (T-2, F-77). A value and a message carry content from the
    /// run, so those keep the replacement and never throw (F-73).
    /// </remarks>
    private void AddField(string name, string value, bool quoted)
    {
        if (name.Length == 0)
        {
            throw new ContextException("A log field needs a name, and this one is empty.");
        }

        // The logger writes the first two names itself, and the assertion report writes the other three. A
        // caller field of any of them would put two of one name in the object, and a reader could then take
        // either value. A repeated call-site name would also stop the report that D-112 requires (F-72, F-74).
        if (name == LevelName || name == MessageName
            || name == AssertFileName || name == AssertLineName || name == AssertMemberName)
        {
            ContextException reserved = new($"The log field name '{name}' belongs to the logger, and no caller field takes it.");
            reserved.AddContext("field", name);
            throw reserved;
        }

        for (int index = 0; index < name.Length; index++)
        {
            char letter = name[index];
            bool isHighSurrogate = letter >= '\uD800' && letter <= '\uDBFF';
            bool isLowSurrogate = letter >= '\uDC00' && letter <= '\uDFFF';
            if (!isHighSurrogate && !isLowSurrogate)
            {
                continue;
            }

            // A high surrogate with its low partner is one character, and the name keeps it.
            if (isHighSurrogate && index + 1 < name.Length && name[index + 1] >= '\uDC00' && name[index + 1] <= '\uDFFF')
            {
                index++;
                continue;
            }

            ContextException invalid = new("A log field name must hold valid text, and this one holds a surrogate without its pair.");
            invalid.AddContext("position", ((long)index).ToString(CultureInfo.InvariantCulture));
            throw invalid;
        }

        foreach (LogField field in this.fields)
        {
            if (field.Name == name)
            {
                ContextException repeated = new($"The log field '{name}' is already present, and a JSON object with two equal names has no defined reading.");
                repeated.AddContext("field", name);
                throw repeated;
            }
        }

        this.fields.Add(new LogField(name, value, quoted));
    }
}

/// <summary>One named value of a log line. <c>Quoted</c> says whether the JSON value needs quotation marks.</summary>
public readonly record struct LogField(string Name, string Value, bool Quoted);
