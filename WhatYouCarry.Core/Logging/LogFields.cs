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
    private readonly List<LogField> fields = [];

    /// <summary>Every field, in the order that the caller added it.</summary>
    public IReadOnlyList<LogField> Fields => this.fields;

    /// <summary>Adds a whole-number field, such as a seed, a floor, or a tick.</summary>
    public void Add(string name, long value)
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

    private void AddField(string name, string value, bool quoted)
    {
        if (name.Length == 0)
        {
            throw new ContextException("A log field needs a name, and this one is empty.");
        }

        if (this.Has(name))
        {
            ContextException error = new($"The log field '{name}' is already present, and a JSON object with two equal names has no defined reading.");
            error.AddContext("field", name);
            throw error;
        }

        this.fields.Add(new LogField(name, value, quoted));
    }
}

/// <summary>One named value of a log line. <c>Quoted</c> says whether the JSON value needs quotation marks.</summary>
public readonly record struct LogField(string Name, string Value, bool Quoted);
