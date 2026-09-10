using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Physics;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Simulation;

namespace WhatYouCarry.Core.Bots;

/// <summary>
/// The greedy descender (D-127, D-149): it walks the reachability path from its floor cell to the stairwell,
/// descends at every stairwell, and ascends at the stairwell of the last floor, so its run ends at the bottom
/// (D-270). It promises progress, so a floor that passes its budget ends the run as a softlock.
/// </summary>
/// <remarks>
/// The walk goes waypoint by waypoint along the path of <see cref="Reachability"/>. A step up is a jump in
/// place, then a move once the feet clear the step (D-165). A drop is a walk off the edge. The look stays at
/// yaw zero, so forward is minus Z and right is plus X (D-234), and the movement bytes point at the next cell.
/// The policy draws nothing, so it holds no stream.
/// </remarks>
public sealed class GreedyDescender : IBotPolicy
{
    /// <summary>The name of the policy in the run log.</summary>
    public const string PolicyName = "greedy-descender";

    /// <summary>How near the feet center must come to the center of a waypoint cell, in meters.</summary>
    public const float Arrival = 0.02f;

    private readonly int lastFloor;
    private IReadOnlyList<Cell> path = [];
    private int pathFloor;
    private int waypoint;

    /// <summary>A descender that ascends at the stairwell of the deepest floor that the content covers (D-3, D-252).</summary>
    public GreedyDescender(ContentSet content)
    {
        int deepest = 0;
        foreach (FloorTemplate template in content.Floors)
        {
            if (template.MaxDepth > deepest)
            {
                deepest = (int)template.MaxDepth;
            }
        }

        this.lastFloor = deepest;
    }

    /// <inheritdoc/>
    public string Name => PolicyName;

    /// <inheritdoc/>
    public bool PromisesProgress => true;

    /// <summary>The floor cell under the feet center of a body.</summary>
    public static Cell FloorCellOf(PlayerBody body)
    {
        return new Cell(
            (int)DetMath.Floor(body.Position.X),
            (int)DetMath.Floor(body.Position.Y - PlayerBody.GroundProbe),
            (int)DetMath.Floor(body.Position.Z));
    }

    /// <inheritdoc/>
    public Intent Next(SimulationLoop loop)
    {
        if (loop.Floor != this.pathFloor)
        {
            Reachability reach = Reachability.From(loop.Grid, FloorCellOf(loop.Body));
            this.path = reach.PathTo(loop.Plan.Stairwell);
            this.pathFloor = loop.Floor;
            this.waypoint = 1;
        }

        while (this.waypoint < this.path.Count && this.Arrived(loop.Body, this.path[this.waypoint]))
        {
            this.waypoint++;
        }

        if (this.waypoint >= this.path.Count)
        {
            ushort choice = loop.Floor >= this.lastFloor ? Button.Ascend : Button.Interact;
            return new Intent(loop.Tick, 0, 0, 0, 0, choice);
        }

        Cell next = this.path[this.waypoint];
        bool stepUp = next.Y == this.path[this.waypoint - 1].Y + 1;
        Cell here = FloorCellOf(loop.Body);
        if (stepUp && loop.Body.IsOnGround() && here.Y < next.Y)
        {
            return new Intent(loop.Tick, 0, 0, 0, 0, Button.Jump);
        }

        if (stepUp && !loop.Body.IsOnGround() && loop.Body.Position.Y < next.Y + 1.0f)
        {
            return new Intent(loop.Tick, 0, 0, 0, 0, 0);
        }

        // Forward at yaw zero is minus Z, so a positive Z delta is a move backward (D-234).
        float deltaX = next.X + 0.5f - loop.Body.Position.X;
        float deltaZ = next.Z + 0.5f - loop.Body.Position.Z;
        return new Intent(loop.Tick, 0, 0, Toward(deltaX), (sbyte)(-Toward(deltaZ)), 0);
    }

    /// <summary>Answers whether the body stands centered on the cell.</summary>
    private bool Arrived(PlayerBody body, Cell cell)
    {
        float deltaX = cell.X + 0.5f - body.Position.X;
        float deltaZ = cell.Z + 0.5f - body.Position.Z;
        bool centered = DetMath.Abs(deltaX) < Arrival && DetMath.Abs(deltaZ) < Arrival;
        return centered && body.IsOnGround() && FloorCellOf(body) == cell;
    }

    /// <summary>The movement byte that moves the body toward a point on one axis, at the walk speed or less when the point is nearer than one tick of walk.</summary>
    private static sbyte Toward(float delta)
    {
        float perTick = PlayerBody.WalkSpeed * PlayerBody.TickSeconds;
        float fraction = DetMath.Clamp(delta / perTick, -1.0f, 1.0f);
        return (sbyte)DetMath.Floor((fraction * PlayerBody.MoveScale) + 0.5f);
    }
}
