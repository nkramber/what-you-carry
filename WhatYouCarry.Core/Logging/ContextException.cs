using System;
using System.Collections.Generic;
using System.Text;

namespace WhatYouCarry.Core.Logging;

/// <summary>
/// An error that carries its context, and that takes more context at each level that rethrows it (T-2, D-113).
/// </summary>
/// <remarks>
/// A catch block adds what it knows and rethrows the same object, so the outer catch reads every field that
/// every level added. The message holds the context, so a log line and a crash report need no second lookup.
/// </remarks>
public sealed class ContextException : Exception
{
    private readonly List<LogField> context = [];

    /// <summary>An error with a message and no context yet.</summary>
    public ContextException(string message)
        : base(message)
    {
    }

    /// <summary>An error that wraps the error below it.</summary>
    public ContextException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>Every context field, in the order that the catch blocks added it.</summary>
    public IReadOnlyList<LogField> Context => this.context;

    /// <summary>The message, and every context field after it.</summary>
    public override string Message => this.context.Count == 0
        ? base.Message
        : $"{base.Message} {this.ContextText()}";

    /// <summary>
    /// Adds one field to the error. A catch block calls this before it rethrows, so the field reaches every
    /// level above it.
    /// </summary>
    /// <exception cref="ContextException">The name is empty, or a field of this name is already present.</exception>
    public void AddContext(string name, string value)
    {
        if (name.Length == 0)
        {
            throw new ContextException("A context field needs a name, and this one is empty.");
        }

        foreach (LogField field in this.context)
        {
            if (field.Name == name)
            {
                throw new ContextException($"The context field '{name}' is already present on this error.");
            }
        }

        this.context.Add(new LogField(name, value, Quoted: true));
    }

    /// <summary>The context as one readable group, such as <c>[seed=7 floor=3]</c>.</summary>
    private string ContextText()
    {
        StringBuilder builder = new();
        builder.Append('[');
        for (int index = 0; index < this.context.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(' ');
            }

            builder.Append(this.context[index].Name);
            builder.Append('=');
            builder.Append(this.context[index].Value);
        }

        builder.Append(']');
        return builder.ToString();
    }
}
