using System.Collections.Generic;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Combat;
using CoreVector3 = WhatYouCarry.Core.Physics.Vector3;

namespace WhatYouCarry.Game.Animation;

/// <summary>
/// The bone rotations of an enemy or of the Overseer on one tick (D-739): its swing clip at the swing tick of Core
/// during a swing, and the rest pose between swings (D-401). An enemy walks in the rest pose, because no enemy has a
/// walk clip yet.
/// </summary>
public static class EnemyPose
{
    /// <summary>The rotations of every bone that the pose turns, in degrees, by bone name.</summary>
    /// <param name="swingTick">The swing tick of the enemy in Core, or <see cref="Swing.NoSwing"/> between swings.</param>
    /// <param name="clip">The swing clip of the enemy, which lasts the swing ticks of its weapon (D-87, D-741).</param>
    public static IReadOnlyDictionary<string, CoreVector3> Rotations(long swingTick, AnimationClip clip)
    {
        if (swingTick == Swing.NoSwing)
        {
            return BodyPose.RestRotations();
        }

        // The loader of the clip holds its length equal to the ticks of the swing, so the tick fits the clip (D-87).
        return clip.RotationsAt((int)swingTick);
    }
}
