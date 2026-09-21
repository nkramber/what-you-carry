using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;

namespace WhatYouCarry.Core.Simulation;

/// <summary>
/// The worker that digs the next floor during the floor before it (D-72, D-429). It is a pure function of the run
/// seed and the floor number over one content set, and it reads no simulation state.
/// </summary>
/// <remarks>
/// Core approves no threading type (D-207, D-208), so the worker holds no thread. The Game layer calls
/// <see cref="Generate"/> on one task during the floor and gives the plan to the loop with
/// <see cref="SimulationLoop.OfferNextFloor"/> before the descent. A headless run offers nothing, and the loop
/// digs the floor on the simulation thread at the descent. The grid is a pure result, so both paths give one
/// grid, and the state hash of the run does not change.
/// </remarks>
public sealed class NextFloorWorker
{
    private readonly ContentSet content;

    /// <summary>A worker over one content set. The content is an input like the seed, and not state (D-236).</summary>
    public NextFloorWorker(ContentSet content)
    {
        this.content = content;
    }

    /// <summary>Answers whether the content covers the floor, so that a worker can dig it. The stairwell of the deepest floor has no floor under it.</summary>
    public bool Covers(int floor)
    {
        return floor >= SimulationLoop.FirstFloor && floor <= FloorGenerator.DeepestFloor(this.content);
    }

    /// <summary>The dug floor for the run seed and the floor number.</summary>
    /// <exception cref="ContextException">The content cannot dig the floor. The error names the seed and the floor.</exception>
    public FloorPlan Generate(ulong seed, int floor)
    {
        return FloorGenerator.Generate(seed, floor, this.content);
    }
}
