using System.Runtime.CompilerServices;

namespace WhatYouCarry.Core.Logging;

/// <summary>
/// The assertion helper. An assertion stays on in a shipped build, and a failure writes a full report (D-112).
/// </summary>
/// <remarks>
/// <para>
/// A failure always writes its report. A caller that marks the call safe then continues, and every other caller
/// takes a <see cref="ContextException"/>. The default is the throw, so a caller decides to continue on purpose
/// and never by omission (T-2).
/// </para>
/// <para>
/// The report names the call site from the compiler, and not from a run-time stack walk. A stack trace changes
/// with the build and the platform, and the file, the line, and the member name do not (G-2).
/// </para>
/// </remarks>
public static class Invariant
{
    /// <summary>
    /// Checks one condition. A failure writes the report to the logger, and it throws unless the caller marks
    /// the call safe.
    /// </summary>
    /// <param name="condition">The condition that must hold.</param>
    /// <param name="message">What the condition means, in the words of the caller.</param>
    /// <param name="logger">The logger that takes the report.</param>
    /// <param name="context">Which required field set the report carries (D-113).</param>
    /// <param name="fields">The context fields of the report. The helper copies them and adds the call site to the copy.</param>
    /// <param name="continueOnFailure">True where the caller can carry on after the report (D-112).</param>
    /// <param name="callerFile">The compiler fills this in.</param>
    /// <param name="callerLine">The compiler fills this in.</param>
    /// <param name="callerMember">The compiler fills this in.</param>
    /// <exception cref="ContextException">The condition fails, and the caller did not mark the call safe.</exception>
    public static void Assert(
        bool condition,
        string message,
        JsonlLogger logger,
        LogContextKind context,
        LogFields fields,
        bool continueOnFailure = false,
        [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0,
        [CallerMemberName] string callerMember = "")
    {
        if (condition)
        {
            return;
        }

        // The report takes a copy. A safe assertion returns, and the caller then uses the same field set again,
        // so this method must leave that set as it found it (F-72).
        LogFields report = fields.Copy();
        report.Add("assertFile", callerFile);
        report.Add("assertLine", callerLine);
        report.Add("assertMember", callerMember);
        logger.Write(context, LogLevel.Error, message, report);

        if (continueOnFailure)
        {
            return;
        }

        ContextException error = new(message);
        foreach (LogField field in report.Fields)
        {
            error.AddContext(field.Name, field.Value);
        }

        throw error;
    }
}
