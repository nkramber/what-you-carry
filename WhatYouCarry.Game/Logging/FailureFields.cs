using System;
using System.Globalization;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Game.Logging;

/// <summary>
/// The fields of an error line that name the exception (D-113, F-121): its text and its type, and then the type and the
/// text of each inner exception, from the outer one in. The text alone could not tell a null reference from a content
/// error, and it lost the cause that a wrapper held.
/// </summary>
/// <remarks>
/// Each inner exception takes a pair of fields with its depth after the name, from one, so the line stays one flat
/// JSON object and every name appears once.
/// </remarks>
public static class FailureFields
{
    /// <summary>The name of the field that holds the text of the exception.</summary>
    public const string ErrorField = "error";

    /// <summary>The name of the field that holds the full type name of the exception.</summary>
    public const string ErrorTypeField = "errorType";

    /// <summary>The start of the name of the field that holds the text of one inner exception. Its depth follows.</summary>
    public const string InnerErrorField = "innerError";

    /// <summary>The start of the name of the field that holds the full type name of one inner exception. Its depth follows.</summary>
    public const string InnerErrorTypeField = "innerErrorType";

    /// <summary>Adds the text and the type of the exception, and the type and the text of each inner exception, to the fields of one line.</summary>
    public static void Add(LogFields fields, Exception error)
    {
        fields.Add(ErrorField, error.Message);
        fields.Add(ErrorTypeField, error.GetType().ToString());
        int depth = 1;
        for (Exception? inner = error.InnerException; inner is not null; inner = inner.InnerException)
        {
            string suffix = depth.ToString(CultureInfo.InvariantCulture);
            fields.Add(InnerErrorTypeField + suffix, inner.GetType().ToString());
            fields.Add(InnerErrorField + suffix, inner.Message);
            depth++;
        }
    }
}
