using System.Collections.Generic;
using WhatYouCarry.Core.Entities;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Animation;

/// <summary>
/// The bone rotations of the player body on one frame (D-87, D-331): the stagger clip during a stagger, the roll clip
/// during a roll, and otherwise the walk of <see cref="WalkCycle"/>, with the tracks of the swing clip over it during a
/// swing. Each clip plays at the tick that the player state gives, so the pose and the phase of Core agree (D-87).
/// </summary>
/// <remarks>
/// The walk stays under a swing, because the walk, the sprint, and the jump stay free during a swing (D-324). A clip
/// tick is the count of ticks of the action that ran, so the first frame after a press shows the first tick of the clip.
/// </remarks>
public static class BodyPose
{
    /// <summary>
    /// The rest pose: every bone at zero. An enemy draws in it between swings (D-401, D-739).
    /// </summary>
    public static IReadOnlyDictionary<string, CoreVector3> RestRotations()
    {
        return new Dictionary<string, CoreVector3>();
    }

    /// <summary>The rotations of every bone that the pose turns, in degrees, by bone name.</summary>
    public static IReadOnlyDictionary<string, CoreVector3> Rotations(Player player, PlayerClips clips, float walked, float walkAmount)
    {
        if (player.StaggerRemaining > 0)
        {
            return clips.Stagger.RotationsAt(Player.StaggerTicks - player.StaggerRemaining);
        }

        if (player.RollRemaining > 0)
        {
            return clips.Dodge.RotationsAt(Player.RollTicks - player.RollRemaining);
        }

        Dictionary<string, CoreVector3> pose = WalkCycle.Rotations(walked, walkAmount);
        if (player.SwingTick != Player.NoSwing)
        {
            // A test holds the length of the swing clip equal to the ticks of the swing of the weapon file, so the tick fits
            // the clip (D-87). A swift windup is shorter, and the clip plays its windup in the shorter time (D-750).
            long clipTick = SwingClipTick(player.SwingTick, player.Equipment.Weapon.WindupTicks, player.Weapon.WindupTicks);
            foreach (KeyValuePair<string, CoreVector3> track in clips.Swing.RotationsAt((int)clipTick))
            {
                pose[track.Key] = track.Value;
            }
        }

        return pose;
    }

    /// <summary>
    /// The tick of the swing clip at a tick of the swing (D-87, D-750). The clip has the windup of the weapon file. A
    /// swing with a shorter windup reads the windup of the clip at the rate of the two windups, rounded down, and every
    /// tick after the windup at the same distance from the end of the windup. A swing with no swift reads the clip
    /// tick for tick.
    /// </summary>
    /// <param name="swingTick">The tick of the swing, from zero.</param>
    /// <param name="clipWindup">The windup ticks of the weapon file, which the clip holds.</param>
    /// <param name="swingWindup">The windup ticks of the swing, which swift can shorten. It is from 1 to the clip windup.</param>
    public static long SwingClipTick(long swingTick, long clipWindup, long swingWindup)
    {
        if (swingTick < swingWindup)
        {
            return swingTick * clipWindup / swingWindup;
        }

        return swingTick + (clipWindup - swingWindup);
    }
}
