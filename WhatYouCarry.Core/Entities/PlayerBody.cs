using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Entities;

/// <summary>
/// The box that stands for the player in Phase 1 (D-149, D-165). It reads the movement, the jump bit, and the
/// sprint bit of an intent, falls under gravity, and moves through <see cref="SweptAabb"/>. No health and no
/// weapon: PR-15 adds those.
/// </summary>
/// <remarks>
/// <para>
/// The position is the feet center: the middle of the box on X and Z, and the bottom of it on Y, in float
/// meters (D-70, D-235). The box is 0.6 by 1.8 by 0.6 meters (D-165). The state is the position and the
/// vertical velocity. The horizontal velocity comes from the intent on every tick, on the ground and in the
/// air, so it is not state (D-233, D-238).
/// </para>
/// <para>
/// The body stands on the ground when a solid cell lies within two skins below its feet (D-235). That is a
/// probe of the grid and not a stored flag, so the state holds nothing that the position does not already
/// say. A jump needs the ground, and a jump clears one block and never two (D-231, D-165).
/// </para>
/// <para>
/// While the feet stand in a still water cell, the walk and the sprint take half their speed, the jump velocity
/// takes one half, and gravity takes one quarter, so the apex of a jump stays at one block and the rise takes
/// twice as long (D-258, D-261, D-262). The probe reads the column of the feet at the start of the tick, and it
/// holds while the body is in the air over the water, because a jump that lost the quarter gravity as soon as
/// the feet rose out of the cell would end below one block. A body that steps off a ledge over a pool falls
/// slowly into it for the same reason.
/// </para>
/// </remarks>
public sealed class PlayerBody
{
    /// <summary>Half the width of the box, on X and on Z, in meters (D-165).</summary>
    public const float HalfWidth = 0.3f;

    /// <summary>The height of the box, in meters (D-165).</summary>
    public const float Height = 1.8f;

    /// <summary>The downward acceleration, in meters per second squared (D-231).</summary>
    public const float Gravity = 20.0f;

    /// <summary>The speed of a walk, in meters per second (D-231).</summary>
    public const float WalkSpeed = 4.0f;

    /// <summary>The speed of a sprint, in meters per second (D-231).</summary>
    public const float SprintSpeed = 6.5f;

    /// <summary>The upward velocity at the start of a jump, in meters per second (D-231).</summary>
    public const float JumpVelocity = 7.0f;

    /// <summary>The length of one tick, in seconds (D-73).</summary>
    public const float TickSeconds = 1.0f / SimulationLoop.TicksPerSecond;

    /// <summary>How far below the feet the ground probe reads, in meters: two skins, so a resting body always finds its block (D-235).</summary>
    public const float GroundProbe = 2.0f * SweptAabb.ContactSkin;

    /// <summary>The largest magnitude of a movement byte. A byte of -128 clamps to -127 (D-233).</summary>
    public const int MoveScale = 127;

    /// <summary>The factor on the walk and the sprint speed while the feet stand in water (D-261).</summary>
    public const float WaterSpeedFactor = 0.5f;

    /// <summary>The factor on the jump velocity while the feet stand in water (D-262).</summary>
    public const float WaterJumpFactor = 0.5f;

    /// <summary>The factor on gravity while the feet stand in water, the square of the jump factor, so the apex stays at one block (D-262).</summary>
    public const float WaterGravityFactor = 0.25f;

    // Hundredths of a degree to radians. The yaw sum stays below 36000, so the angle stays far below the
    // DetMath limit.
    private const float RadiansPerHundredth = DetMath.Pi / 18000.0f;

    private readonly VoxelGrid grid;

    /// <summary>A body at rest at the spawn point, which is a feet center.</summary>
    /// <exception cref="ContextException">The box at the spawn point overlaps a solid cell or reaches past the grid.</exception>
    public PlayerBody(VoxelGrid grid, Vector3 spawn)
    {
        this.grid = grid;
        this.Position = spawn;

        if (SweptAabb.Overlaps(grid, this.Box))
        {
            ContextException error = new($"The spawn point {spawn} puts the player box inside a solid cell or outside the grid.");
            error.AddContext("spawn", spawn.ToString());
            throw error;
        }
    }

    /// <summary>The feet center, in meters.</summary>
    public Vector3 Position { get; private set; }

    /// <summary>The vertical velocity, in meters per second. Positive is up.</summary>
    public float VerticalVelocity { get; private set; }

    /// <summary>The box of the body at its position.</summary>
    public Aabb Box => new(
        new Vector3(this.Position.X - HalfWidth, this.Position.Y, this.Position.Z - HalfWidth),
        new Vector3(this.Position.X + HalfWidth, this.Position.Y + Height, this.Position.Z + HalfWidth));

    /// <summary>Answers whether a solid cell lies within the ground probe below the feet, across the whole footprint.</summary>
    public bool IsOnGround()
    {
        Aabb box = this.Box;
        int row = (int)DetMath.Floor(box.Min.Y - GroundProbe);
        int lowX = (int)DetMath.Floor(box.Min.X);
        int highX = -(int)DetMath.Floor(-box.Max.X) - 1;
        int lowZ = (int)DetMath.Floor(box.Min.Z);
        int highZ = -(int)DetMath.Floor(-box.Max.Z) - 1;
        return this.grid.IsAnySolid(lowX, highX, row, row, lowZ, highZ);
    }

    /// <summary>
    /// Answers whether the feet cell holds still water, or the body is in the air over still water: the first
    /// block at or under the feet cell that is not air is still water (D-258). A column outside the grid holds none.
    /// </summary>
    public bool IsInWater()
    {
        int x = (int)DetMath.Floor(this.Position.X);
        int z = (int)DetMath.Floor(this.Position.Z);
        for (int y = (int)DetMath.Floor(this.Position.Y); y >= 0; y--)
        {
            if (!this.grid.Contains(x, y, z))
            {
                return false;
            }

            BlockId block = this.grid.Get(x, y, z);
            if (block != BlockId.Air)
            {
                return block == BlockId.StillWater;
            }
        }

        return false;
    }

    /// <summary>
    /// Runs one tick. The jump comes first, then gravity, then the horizontal velocity of the intent, and then
    /// one sweep with the whole displacement. Water scales the three at the start of the tick (D-261, D-262).
    /// </summary>
    /// <param name="intent">The intent of the tick.</param>
    /// <param name="yaw">The yaw sum of the loop after the intent, in hundredths of a degree (D-227).</param>
    public void Step(Intent intent, int yaw)
    {
        bool inWater = this.IsInWater();
        float jumpFactor = inWater ? WaterJumpFactor : 1.0f;
        float gravityFactor = inWater ? WaterGravityFactor : 1.0f;
        float speedFactor = inWater ? WaterSpeedFactor : 1.0f;

        if ((intent.Buttons & Button.Jump) != 0 && this.IsOnGround())
        {
            this.VerticalVelocity = JumpVelocity * jumpFactor;
        }

        this.VerticalVelocity -= Gravity * gravityFactor * TickSeconds;

        Vector3 horizontal = HorizontalVelocity(intent, yaw) * speedFactor;
        Vector3 delta = new(horizontal.X * TickSeconds, this.VerticalVelocity * TickSeconds, horizontal.Z * TickSeconds);

        SweepResult result = SweptAabb.Sweep(this.grid, this.Box, delta);
        this.Position += result.Allowed;

        // A landing and a hit on a ceiling both end the vertical motion.
        if (result.BlockedY)
        {
            this.VerticalVelocity = 0.0f;
        }
    }

    /// <summary>
    /// The horizontal velocity of an intent (D-233). The strafe and the forward fractions clamp to a length of
    /// one, so a diagonal is never faster than a straight line, and the pair rotates by the yaw.
    /// </summary>
    private static Vector3 HorizontalVelocity(Intent intent, int yaw)
    {
        int moveX = intent.MoveX;
        int moveY = intent.MoveY;
        if (moveX < -MoveScale)
        {
            moveX = -MoveScale;
        }

        if (moveY < -MoveScale)
        {
            moveY = -MoveScale;
        }

        float strafe = moveX / (float)MoveScale;
        float forward = moveY / (float)MoveScale;
        float length = DetMath.Sqrt((strafe * strafe) + (forward * forward));
        if (length > 1.0f)
        {
            strafe /= length;
            forward /= length;
        }

        float speed = (intent.Buttons & Button.Sprint) != 0 ? SprintSpeed : WalkSpeed;

        // Forward at yaw zero is minus Z, and right is plus X. The yaw turns counterclockwise seen from above,
        // so forward at 90 degrees is minus X and right is minus Z (D-234).
        float radians = yaw * RadiansPerHundredth;
        float sin = DetMath.Sin(radians);
        float cos = DetMath.Cos(radians);
        float x = ((strafe * cos) - (forward * sin)) * speed;
        float z = ((-strafe * sin) - (forward * cos)) * speed;
        return new Vector3(x, 0.0f, z);
    }
}
