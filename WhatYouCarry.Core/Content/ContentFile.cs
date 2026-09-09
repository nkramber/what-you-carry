using System.Collections.Generic;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One content file: its path relative to the content root, and its bytes (D-219).
/// </summary>
/// <remarks>
/// The path uses a forward slash on every platform, so the content hash of D-163 gives one answer everywhere.
/// Core never reads a directory, so the host makes these records and hands them over (D-211, D-219).
/// </remarks>
public sealed record ContentFile(string Path, IReadOnlyList<byte> Bytes);
