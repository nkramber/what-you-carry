using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Entities;

/// <summary>
/// The box that stands for the player (D-149, D-165). It falls under gravity and moves through
/// <see cref="SweptAabb"/>. The <see cref="Player"/> of PR-15 owns it, and it adds health, the roll, the stagger,
/// and the swing.
/// </summary>
/// <remarks>
/// <para>
/// The position is the feet center: the middle of the box on X and Z, and the bottom of it on Y, in float
/// meters (D-70, D-235). The box is 0.6 by 1.8 by 0.6 meters (D-165). The state is the position and the
/// vertical velocity. The horizontal velocity comes from the caller on every tick, on the ground and in the
/// air, so it is not state (D-233, D-238).
/// </para>
/// <para>
/// The body stands on the ground when a solid part lies within two skins below its feet: the top of a block, or
/// the slope of a ramp (D-235, D-365). That is a probe of the grid and not a stored flag, so the state holds
/// nothing that the position does not already say. A jump needs the ground, and a jump clears one block and never
/// two (D-231, D-165).
/// </para>
/// <para>
/// On a tick that starts on the ground over a ramp, the horizontal velocity takes the factor that keeps its speed
/// along the slope (D-362). The sweep lifts the body up a slope. A walk and a sprint also follow a slope down, and
/// a roll leaves it, so gravity takes over (D-363, D-366). A body with no horizontal velocity stays where it stands
/// on a slope (D-364).
/// </para>
/// <para>
/// While the feet stand in a still water cell, the walk and the sprint take half their speed, the jump velocity
/// takes one half, and gravity takes one quarter, so the apex of a jump stays at one block and the rise takes
/// twice as long (D-258, D-261, D-262). The probe reads the column of the feet at the start of the tick, and it
/// holds while the body is in the air over the water, because a jump that lost the quarter gravity as soon as
/// the feet rose out of the cell would end below one block. A body that steps off a ledge over a pool falls
/// slowly into it for the same reason. A roll keeps its own speed over water (D-337).
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

    /// <summary>The speed of a sprint, in meters per second (D-315, D-319).</summary>
    public const float SprintSpeed = 7.0f;

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
    /// <exception cref="ContextException">The box at the spawn point overlaps a solid part or reaches past the grid.</exception>
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

    /// <summary>
    /// Answers whether a solid part lies within the ground probe below the feet, across the whole footprint: the top
    /// of a block or the slope of a ramp (D-235, D-365). The rows run from the row of the probe to the row of the
    /// feet, because the slope of a ramp in the feet row can hold the body.
    /// </summary>
    public bool IsOnGround()
    {
        Aabb box = this.Box;
        float probe = box.Min.Y - GroundProbe;
        int highRow = (int)DetMath.Floor(box.Min.Y);
        for (int row = (int)DetMath.Floor(probe); row <= highRow; row++)
        {
            if (this.grid.TryTopUnder(row, box.Min.X, box.Max.X, box.Min.Z, box.Max.Z, out float top) && top >= probe)
            {
                return true;
            }
        }

        return false;
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
    /// Runs one tick of a walk: the horizontal velocity of the intent, with the water factor of the start of the
    /// tick, and the jump bit (D-233, D-261). The walk follows a slope (D-363). The player of PR-15 calls
    /// <see cref="Move"/> directly.
    /// </summary>
    /// <param name="intent">The intent of the tick.</param>
    /// <param name="yaw">The yaw sum of the loop after the intent, in hundredths of a degree (D-227).</param>
    public void Step(Intent intent, int yaw)
    {
        bool inWater = this.IsInWater();
        float speedFactor = inWater ? WaterSpeedFactor : 1.0f;
        this.Move(WalkVelocity(intent, yaw) * speedFactor, (intent.Buttons & Button.Jump) != 0, inWater, true);
    }

    /// <summary>
    /// Runs one tick with a horizontal velocity. On the ground over a ramp, the velocity first takes the slope
    /// factor (D-362). The jump comes next, then gravity, and then one sweep with the whole displacement. Water
    /// scales the jump and gravity (D-262). The caller scales the horizontal velocity for water. A body that follows
    /// the slope and leaves the ground in the sweep then moves down onto the slope, by at most the drop of a slope of
    /// 1:2 over the step, so a body still falls from a ledge of one block (D-363).
    /// </summary>
    /// <param name="horizontal">The horizontal velocity of the tick, in meters per second. Its Y component is zero.</param>
    /// <param name="jump">Whether the tick asks for a jump. A jump needs the ground.</param>
    /// <param name="inWater">The water probe of the start of the tick (D-264).</param>
    /// <param name="followSlope">Whether the body stays on a slope on the way down: true for a walk, a sprint, and a stagger, and false for a roll (D-363, D-364, D-366).</param>
    public void Move(Vector3 horizontal, bool jump, bool inWater, bool followSlope)
    {
        float jumpFactor = inWater ? WaterJumpFactor : 1.0f;
        float gravityFactor = inWater ? WaterGravityFactor : 1.0f;

        bool onGround = this.IsOnGround();
        Vector3 velocity = onGround ? this.AlongSlope(horizontal) : horizontal;
        bool jumped = jump && onGround;
        if (jumped)
        {
            this.VerticalVelocity = JumpVelocity * jumpFactor;
        }

        this.VerticalVelocity -= Gravity * gravityFactor * TickSeconds;

        Vector3 delta = new(velocity.X * TickSeconds, this.VerticalVelocity * TickSeconds, velocity.Z * TickSeconds);
        SweepResult result = SweptAabb.Sweep(this.grid, this.Box, delta);
        this.Position += result.Allowed;

        // A landing and a hit on a ceiling both end the vertical motion.
        if (result.BlockedY)
        {
            this.VerticalVelocity = 0.0f;
        }

        if (!followSlope || !onGround || jumped || this.IsOnGround())
        {
            return;
        }

        // A step down a slope leaves the feet over the slope by the drop of the step. A sweep down by the largest
        // such drop finds the slope, and a sweep that finds nothing moves nothing, so the body falls as before.
        float step = DetMath.Abs(result.Allowed.X) + DetMath.Abs(result.Allowed.Z);
        float reach = (step / Ramp.SteepestRun) + GroundProbe;
        SweepResult down = SweptAabb.Sweep(this.grid, this.Box, new Vector3(0.0f, -reach, 0.0f));
        if (down.BlockedY)
        {
            this.Position += down.Allowed;
            this.VerticalVelocity = 0.0f;
        }
    }

    /// <summary>
    /// The horizontal velocity of the walk (D-233): the walk speed, or the sprint speed when the sprint bit is set. The
    /// strafe and the forward fractions clamp to a length of one, so a diagonal is never faster than a straight line,
    /// and the pair rotates by the yaw.
    /// </summary>
    public static Vector3 WalkVelocity(Intent intent, int yaw)
    {
        float strafe = Fraction(intent.MoveX);
        float forward = Fraction(intent.MoveY);
        float length = DetMath.Sqrt((strafe * strafe) + (forward * forward));
        if (length > 1.0f)
        {
            strafe /= length;
            forward /= length;
        }

        float speed = (intent.Buttons & Button.Sprint) != 0 ? SprintSpeed : WalkSpeed;
        return YawFrame(strafe, forward, yaw, speed);
    }

    /// <summary>
    /// The unit direction of a roll (D-327): the direction of the movement input, rotated by the yaw, or backward
    /// with no input. The input sets the direction and never the distance, so a small stick deflection rolls as far
    /// as a full one.
    /// </summary>
    public static Vector3 RollDirection(Intent intent, int yaw)
    {
        float strafe = Fraction(intent.MoveX);
        float forward = Fraction(intent.MoveY);
        float length = DetMath.Sqrt((strafe * strafe) + (forward * forward));
        if (length == 0.0f)
        {
            return YawFrame(0.0f, -1.0f, yaw, 1.0f);
        }

        return YawFrame(strafe / length, forward / length, yaw, 1.0f);
    }

    /// <summary>
    /// A horizontal velocity scaled so that its speed along the slope under the feet center is its horizontal speed
    /// (D-362). The body rises 1 / run meters per meter along the rise, so the speed along the slope is the length of
    /// the horizontal velocity and the rise speed together. A velocity with no ramp under the feet center, in the rows
    /// from the ground probe to the feet, stays as it is.
    /// </summary>
    private Vector3 AlongSlope(Vector3 horizontal)
    {
        int x = (int)DetMath.Floor(this.Position.X);
        int z = (int)DetMath.Floor(this.Position.Z);
        int highRow = (int)DetMath.Floor(this.Position.Y);
        for (int row = (int)DetMath.Floor(this.Position.Y - GroundProbe); row <= highRow; row++)
        {
            if (!this.grid.TryGetRamp(x, row, z, out Ramp ramp))
            {
                continue;
            }

            float flatSquared = (horizontal.X * horizontal.X) + (horizontal.Z * horizontal.Z);
            if (flatSquared == 0.0f)
            {
                return horizontal;
            }

            float rise = (ramp.RisesAlongX ? horizontal.X : horizontal.Z) / ramp.Run;
            return horizontal * DetMath.Sqrt(flatSquared / (flatSquared + (rise * rise)));
        }

        return horizontal;
    }

    /// <summary>One movement byte as a fraction from -1 to 1. A byte of -128 clamps to -127 (D-233).</summary>
    private static float Fraction(sbyte move)
    {
        int value = move < -MoveScale ? -MoveScale : move;
        return value / (float)MoveScale;
    }

    /// <summary>
    /// A strafe and a forward amount in the frame of the yaw, scaled. Forward at yaw zero is minus Z, and right is
    /// plus X. The yaw turns counterclockwise seen from above, so forward at 90 degrees is minus X and right is minus
    /// Z (D-234).
    /// </summary>
    private static Vector3 YawFrame(float strafe, float forward, int yaw, float scale)
    {
        float radians = yaw * RadiansPerHundredth;
        float sin = DetMath.Sin(radians);
        float cos = DetMath.Cos(radians);
        float x = ((strafe * cos) - (forward * sin)) * scale;
        float z = ((-strafe * sin) - (forward * cos)) * scale;
        return new Vector3(x, 0.0f, z);
    }
}
