using System.Collections.Generic;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Assets;

/// <summary>
/// One animation of one model (D-87, D-298): a rotation track per bone, in ticks at 60 Hz, and a phase tag
/// over every tick range. The file lives next to its model.
/// </summary>
/// <param name="Model">The content path of the model, as the file names it.</param>
/// <param name="Name">The animation name: the part of the file name between the model stem and the extension.</param>
/// <param name="Length">The count of ticks. The last tick of the animation is this number.</param>
/// <param name="Tracks">One track per bone that the animation turns, in file order.</param>
/// <param name="Phases">The phase ranges, in tick order. Together they cover tick zero to the length.</param>
public sealed record AnimationClip(string Model, string Name, int Length, IReadOnlyList<BoneTrack> Tracks, IReadOnlyList<PhaseRange> Phases)
{
    /// <summary>Every tick that holds a keyframe on any bone, in ascending order, each once.</summary>
    public IReadOnlyList<int> KeyframeTicks()
    {
        SortedSet<int> ticks = [];
        foreach (BoneTrack track in this.Tracks)
        {
            foreach (Keyframe keyframe in track.Keyframes)
            {
                ticks.Add(keyframe.Tick);
            }
        }

        return [.. ticks];
    }

    /// <summary>The rotation of every bone with a track at one tick, in degrees. The track interpolates between its keyframes.</summary>
    public IReadOnlyDictionary<string, Vector3> RotationsAt(int tick)
    {
        Dictionary<string, Vector3> rotations = [];
        foreach (BoneTrack track in this.Tracks)
        {
            rotations.Add(track.Bone, track.RotationAt(tick));
        }

        return rotations;
    }
}

/// <summary>The keyframes of one bone, in ascending tick order.</summary>
/// <param name="Bone">The bone name in the model.</param>
/// <param name="Keyframes">At least one keyframe, in ascending tick order, no two at one tick.</param>
public sealed record BoneTrack(string Bone, IReadOnlyList<Keyframe> Keyframes)
{
    /// <summary>
    /// The rotation at one tick, in degrees. Between two keyframes the track is linear in each angle (D-298).
    /// Before the first keyframe the track holds the first, and after the last it holds the last.
    /// </summary>
    public Vector3 RotationAt(int tick)
    {
        Keyframe first = this.Keyframes[0];
        if (tick <= first.Tick)
        {
            return first.RotationDegrees;
        }

        for (int index = 1; index < this.Keyframes.Count; index++)
        {
            Keyframe next = this.Keyframes[index];
            if (tick > next.Tick)
            {
                continue;
            }

            Keyframe previous = this.Keyframes[index - 1];
            float fraction = (float)(tick - previous.Tick) / (next.Tick - previous.Tick);
            return previous.RotationDegrees + ((next.RotationDegrees - previous.RotationDegrees) * fraction);
        }

        return this.Keyframes[this.Keyframes.Count - 1].RotationDegrees;
    }
}

/// <summary>One keyframe: the bone rotation at one tick, in euler degrees in the order of Blockbench (D-298).</summary>
public readonly record struct Keyframe(int Tick, Vector3 RotationDegrees);

/// <summary>One phase range: the ticks from <paramref name="Start"/> up to and excluding <paramref name="End"/>, with a tag of <see cref="PhaseTags"/>.</summary>
public readonly record struct PhaseRange(int Start, int End, string Tag);

/// <summary>The phase tags of D-298. A test of D-87 compares the ranges with the tick counts of the weapon.</summary>
public static class PhaseTags
{
    /// <summary>The ticks before the hit box is live.</summary>
    public const string Windup = "windup";

    /// <summary>The ticks with the hit box live.</summary>
    public const string Active = "active";

    /// <summary>The ticks after the hit box, before the next action.</summary>
    public const string Recovery = "recovery";

    /// <summary>The ticks with no attack.</summary>
    public const string Idle = "idle";

    /// <summary>Every tag.</summary>
    public static readonly IReadOnlyList<string> Names = [Windup, Active, Recovery, Idle];

    /// <summary>Answers whether a text is a tag.</summary>
    public static bool Contains(string tag)
    {
        foreach (string name in Names)
        {
            if (name == tag)
            {
                return true;
            }
        }

        return false;
    }
}
