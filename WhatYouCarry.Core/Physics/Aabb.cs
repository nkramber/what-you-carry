namespace WhatYouCarry.Core.Physics;

/// <summary>
/// An axis-aligned box: the corner with the smallest coordinates and the corner with the largest ones.
/// </summary>
/// <remarks>
/// A box covers the open interval (Min, Max) on each axis. A face that touches a block face is contact and not
/// overlap, so a box that rests on a block overlaps nothing.
/// </remarks>
public readonly record struct Aabb(Vector3 Min, Vector3 Max)
{
    /// <summary>The same box moved by a displacement.</summary>
    public Aabb Moved(Vector3 delta)
    {
        return new Aabb(this.Min + delta, this.Max + delta);
    }
}
