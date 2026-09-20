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
    /// The rest pose: every bone at zero. An enemy of PR-16 draws in it, because PR-16 plays no clip for an enemy
    /// (D-401).
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
            // A test holds the length of the swing clip equal to the ticks of the swing, so the tick fits the clip (D-87).
            foreach (KeyValuePair<string, CoreVector3> track in clips.Swing.RotationsAt((int)player.SwingTick))
            {
                pose[track.Key] = track.Value;
            }
        }

        return pose;
    }
}
