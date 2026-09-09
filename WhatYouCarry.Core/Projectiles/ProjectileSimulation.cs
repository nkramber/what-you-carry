using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Projectiles;

/// <summary>
/// The projectiles of one floor (G-6, D-30): a flat list in flight order, fixed-step Euler integration under
/// the gravity of D-231 scaled by the definition, a lifetime in ticks, a spread from the Projectile stream, and
/// a swept collision of each tick against the grid and the entity boxes.
/// </summary>
/// <remarks>
/// <para>
/// A projectile is a point. On each tick it ages, gravity changes its velocity, and it moves by one tick of
/// that velocity. The move is one segment, and the grid ray march finds the first solid cell on it, so a fast
/// shot never passes through a wall between two ticks. An entity box is a slab test on the same segment. The
/// nearest hit ends the projectile at its point, and a projectile that outlives its definition ends where it is.
/// </para>
/// <para>
/// The list holds no spatial partition (D-109). A tick reads every projectile once and every entity box once
/// per projectile, and the counts stay small in Phase 1.
/// </para>
/// </remarks>
public sealed class ProjectileSimulation
{
    /// <summary>The length of one tick, in seconds (D-73).</summary>
    public const float TickSeconds = PlayerBody.TickSeconds;

    // Hundredths of a degree to radians.
    private const float RadiansPerHundredth = DetMath.Pi / 18000.0f;

    private readonly VoxelGrid grid;
    private readonly IReadOnlyList<ProjectileDefinition> definitions;
    private List<Projectile> live = [];

    /// <summary>A simulation with no projectile in flight, over one grid and the definitions of the content set.</summary>
    public ProjectileSimulation(VoxelGrid grid, IReadOnlyList<ProjectileDefinition> definitions)
    {
        this.grid = grid;
        this.definitions = definitions;
    }

    /// <summary>The projectiles in flight, in flight order.</summary>
    public IReadOnlyList<Projectile> Live => this.live;

    /// <summary>The definitions that the simulation fires, in the order of the content set.</summary>
    public IReadOnlyList<ProjectileDefinition> Definitions => this.definitions;

    /// <summary>
    /// Fires one projectile of a definition from a point along a direction, with the spread of the definition
    /// drawn from the stream: one yaw offset and one pitch offset, each uniform inside the half angle (D-266).
    /// </summary>
    /// <exception cref="ContextException">The definition index is outside the list, or the direction has no length.</exception>
    public void Fire(int definition, int owner, Vector3 origin, Vector3 direction, Rng rng)
    {
        if (definition < 0 || definition >= this.definitions.Count)
        {
            ContextException error = new($"The projectile definition index {definition} is outside the {this.definitions.Count} definitions of the content set.");
            error.AddContext("definition", ((long)definition).ToString(CultureInfo.InvariantCulture));
            error.AddContext("definitions", ((long)this.definitions.Count).ToString(CultureInfo.InvariantCulture));
            throw error;
        }

        float length = direction.Length();
        if (length == 0.0f)
        {
            ContextException error = new($"A shot from {origin} needs a direction with a length, and the direction is {direction}.");
            error.AddContext("origin", origin.ToString());
            throw error;
        }

        ProjectileDefinition shape = this.definitions[definition];
        Vector3 unit = direction * (1.0f / length);
        if (shape.SpreadHundredths > 0)
        {
            int yawOffset = rng.NextInt((2 * shape.SpreadHundredths) + 1) - shape.SpreadHundredths;
            int pitchOffset = rng.NextInt((2 * shape.SpreadHundredths) + 1) - shape.SpreadHundredths;
            unit = Turn(unit, yawOffset * RadiansPerHundredth, pitchOffset * RadiansPerHundredth);
        }

        float speed = shape.SpeedCentimetres / 100.0f;
        this.live.Add(new Projectile(definition, owner, origin, unit * speed, 0));
    }

    /// <summary>
    /// Runs one tick for every projectile, and gives the projectiles that ended on it, in flight order. The
    /// survivors keep their order.
    /// </summary>
    public IReadOnlyList<ProjectileEnd> Step(IReadOnlyList<EntityBox> boxes)
    {
        List<Projectile> survivors = [];
        List<ProjectileEnd> ends = [];
        foreach (Projectile projectile in this.live)
        {
            ProjectileDefinition shape = this.definitions[projectile.Definition];
            int age = projectile.Age + 1;
            if (age > shape.LifetimeTicks)
            {
                ends.Add(new ProjectileEnd(projectile, ProjectileEndKind.Lifetime, projectile.Position, -1));
                continue;
            }

            float gravity = PlayerBody.Gravity * (shape.GravityScalePercent / 100.0f);
            Vector3 velocity = new(projectile.Velocity.X, projectile.Velocity.Y - (gravity * TickSeconds), projectile.Velocity.Z);
            Vector3 delta = velocity * TickSeconds;
            Vector3 end = projectile.Position + delta;

            float nearest = 2.0f;
            ProjectileEndKind kind = ProjectileEndKind.Lifetime;
            int entityIndex = -1;
            RayHit wall = GridRay.FirstSolid(this.grid, projectile.Position, end);
            float length = delta.Length();
            if (wall.Hit && length > 0.0f)
            {
                nearest = wall.Distance / length;
                kind = ProjectileEndKind.Grid;
            }

            for (int index = 0; index < boxes.Count; index++)
            {
                if (boxes[index].Owner == projectile.Owner)
                {
                    continue;
                }

                float entry = SegmentEntry(projectile.Position, delta, boxes[index].Box);
                if (entry >= 0.0f && entry < nearest)
                {
                    nearest = entry;
                    kind = ProjectileEndKind.Entity;
                    entityIndex = index;
                }
            }

            if (nearest <= 1.0f)
            {
                Vector3 point = projectile.Position + (delta * nearest);
                ends.Add(new ProjectileEnd(new Projectile(projectile.Definition, projectile.Owner, point, velocity, age), kind, point, entityIndex));
                continue;
            }

            survivors.Add(new Projectile(projectile.Definition, projectile.Owner, end, velocity, age));
        }

        this.live = survivors;
        return ends;
    }

    /// <summary>Folds every projectile into the hash, in flight order, after their count (D-160).</summary>
    public void AddTo(ref StateHash hash)
    {
        hash.Add(this.live.Count);
        foreach (Projectile projectile in this.live)
        {
            hash.Add(projectile.Definition);
            hash.Add(projectile.Owner);
            hash.Add(projectile.Position.X);
            hash.Add(projectile.Position.Y);
            hash.Add(projectile.Position.Z);
            hash.Add(projectile.Velocity.X);
            hash.Add(projectile.Velocity.Y);
            hash.Add(projectile.Velocity.Z);
            hash.Add(projectile.Age);
        }
    }

    /// <summary>
    /// The segment parameter, from 0 to 1, where a segment enters a box, or minus one when it misses. A start
    /// inside the box enters at zero.
    /// </summary>
    public static float SegmentEntry(Vector3 start, Vector3 delta, Aabb box)
    {
        float entry = 0.0f;
        float exit = 1.0f;
        if (!ClipAxis(start.X, delta.X, box.Min.X, box.Max.X, ref entry, ref exit)
            || !ClipAxis(start.Y, delta.Y, box.Min.Y, box.Max.Y, ref entry, ref exit)
            || !ClipAxis(start.Z, delta.Z, box.Min.Z, box.Max.Z, ref entry, ref exit))
        {
            return -1.0f;
        }

        return entry;
    }

    /// <summary>Narrows the entry and exit parameters of a segment to the slab of one axis. False when the slab excludes the whole segment.</summary>
    private static bool ClipAxis(float start, float delta, float low, float high, ref float entry, ref float exit)
    {
        if (delta == 0.0f)
        {
            return start >= low && start <= high;
        }

        float first = (low - start) / delta;
        float second = (high - start) / delta;
        float near = first < second ? first : second;
        float far = first < second ? second : first;
        if (near > entry)
        {
            entry = near;
        }

        if (far < exit)
        {
            exit = far;
        }

        return entry <= exit;
    }

    /// <summary>
    /// Turns a unit direction by a yaw offset and a pitch offset, in radians. The yaw turns counterclockwise seen
    /// from above, and a positive pitch lifts the direction (D-234, D-248). A vertical direction takes yaw zero.
    /// </summary>
    private static Vector3 Turn(Vector3 unit, float yawOffset, float pitchOffset)
    {
        float horizontal = DetMath.Sqrt((unit.X * unit.X) + (unit.Z * unit.Z));
        float yaw = horizontal == 0.0f ? 0.0f : DetMath.Atan2(-unit.X, -unit.Z);
        float pitch = DetMath.Atan2(unit.Y, horizontal);
        float newYaw = yaw + yawOffset;
        float newPitch = DetMath.Clamp(pitch + pitchOffset, -DetMath.HalfPi, DetMath.HalfPi);

        float cosPitch = DetMath.Cos(newPitch);
        return new Vector3(-DetMath.Sin(newYaw) * cosPitch, DetMath.Sin(newPitch), -DetMath.Cos(newYaw) * cosPitch);
    }
}
