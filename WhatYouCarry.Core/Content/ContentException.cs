using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// The error that a content failure raises. It names the file, the field, and the reason (D-92, T-2). The run
/// record header uses the same shape, so the text says "file" and not "content file" (D-229).
/// </summary>
public static class ContentError
{
    /// <summary>An error that names the file, the field, and the reason. Every content failure uses this shape.</summary>
    public static ContextException Make(string path, string field, string reason)
    {
        ContextException error = new($"The file '{path}' is not valid. The field '{field}' {reason}.");
        error.AddContext("file", path);
        error.AddContext("field", field);
        error.AddContext("reason", reason);
        return error;
    }

    /// <summary>An error about a whole file, where no one field is at fault.</summary>
    public static ContextException MakeForFile(string path, string reason)
    {
        ContextException error = new($"The file '{path}' is not valid. {reason}.");
        error.AddContext("file", path);
        error.AddContext("reason", reason);
        return error;
    }
}
