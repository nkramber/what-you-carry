using System.Collections.Generic;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// Where the content bytes come from (D-219). Core opens no file, so a disk failure never reaches the
/// simulation thread, and no path lives in Core (D-211, G-1, OQ-57).
/// </summary>
/// <remarks>
/// The Game layer reads the content directory of the exported build, and a test hands over a set that it holds
/// in memory. Those are the two concrete callers that D-111 asks for before an interface.
/// </remarks>
public interface IContentSource
{
    /// <summary>Every content file. The loader sorts them, so this method needs no order of its own.</summary>
    IReadOnlyList<ContentFile> Read();
}
