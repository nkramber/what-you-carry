using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Godot;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Simulation;
using WhatYouCarry.Core.World;
using WhatYouCarry.Game.Render;

namespace WhatYouCarry.Game.World;

/// <summary>
/// The floor transition of the world meshes (D-72, D-429). During floor n, one task digs floor n+1 with the
/// worker and meshes its chunks. The loop takes the plan, and the meshes of floor n+1 go into hidden nodes a few
/// at a time. At the descent the old nodes go and the new ones show in one frame.
/// </summary>
/// <remarks>
/// <para>
/// The task reads the seed, the floor number, and the content alone, so it touches no state of the loop (D-429).
/// The loop reads the result on the simulation thread through <see cref="SimulationLoop.OfferNextFloor"/>, and
/// the worker gives the same grid as a dig at the descent, so the task changes no tick.
/// </para>
/// <para>
/// The greedy mesher runs on the task and not on the main thread. The first Deck run of exit test 6 traced 38 to 46
/// milliseconds for the four chunks that one frame meshed and uploaded, and the mesher took nearly all of it
/// (D-109, D-427). The mesher reads the grid and writes value types alone, so the task needs no engine call. The
/// main thread builds each engine mesh from the data of the task.
/// </para>
/// <para>
/// A descent that comes before the task or the upload ends builds the rest of the chunks in that frame. The
/// frame is then slow, and the frame log shows it (D-427). A task that failed throws its error at the offer, with
/// the seed and the floor, so no failure of the worker is lost (T-2).
/// </para>
/// </remarks>
public sealed class ChunkSwap
{
    /// <summary>
    /// The count of chunks that one frame uploads. A maximum floor holds 64 chunks (D-291), so the upload takes 16
    /// frames. The Deck trace measured the cost of the mesher, and the mesher now runs on the task, so each chunk of
    /// the main thread is one engine mesh build. A later change needs a measurement on the Deck (D-109).
    /// </summary>
    public const int ChunksPerFrame = 4;

    private const string TaskFailedMessage = "The worker failed to dig the next floor.";
    private const string SeedField = "seed";
    private const string FloorField = "floor";
    private const string CauseField = "cause";

    private readonly Node parent;
    private readonly Material material;
    private readonly NextFloorWorker worker;
    private readonly ulong seed;
    private List<MeshInstance3D> shown = [];
    private int shownFloor;
    private Task<NextFloor>? digging;
    private int diggingFloor;
    private FloorPlan? staged;
    private IReadOnlyList<MeshData> stagedMeshes = [];
    private List<MeshInstance3D> stagedNodes = [];
    private int stagedChunk;

    /// <summary>A swap that adds its nodes under the parent, with the world material on each.</summary>
    public ChunkSwap(Node parent, Material material, NextFloorWorker worker, ulong seed)
    {
        this.parent = parent;
        this.material = material;
        this.worker = worker;
        this.seed = seed;
    }

    /// <summary>The nodes of the floor on screen, in chunk order.</summary>
    public IReadOnlyList<MeshInstance3D> Shown => this.shown;

    /// <summary>Answers whether a task digs a floor now.</summary>
    public bool IsDigging => this.digging is not null && !this.digging.IsCompleted;

    /// <summary>The wall time of the last dig that the loop took, in microseconds, measured on the task. Zero before the first one.</summary>
    public long LastDigMicros { get; private set; }

    /// <summary>The wall time of the meshes of the last floor that the loop took, in microseconds, measured on the task. Zero before the first one.</summary>
    public long LastMeshMicros { get; private set; }

    /// <summary>Answers whether the last swap showed the plan of the worker, and not a floor that the loop dug at the descent.</summary>
    public bool LastSwapFromWorker { get; private set; }

    /// <summary>Answers whether every chunk of the next floor is uploaded and waits in hidden nodes.</summary>
    public bool NextFloorReady => this.staged is not null && this.stagedChunk == this.stagedMeshes.Count;

    /// <summary>Shows the floor of the loop, built in this frame, and starts the task of the next floor.</summary>
    public void Start(SimulationLoop loop)
    {
        this.shown = this.BuildAll(loop.Grid, true);
        this.shownFloor = loop.Floor;
        this.StartDig(loop.Floor + 1);
    }

    /// <summary>
    /// Offers the plan of the next floor to the loop when the task ended and no plan is on offer. Call it before
    /// each tick.
    /// </summary>
    /// <returns>True when this call offered a plan.</returns>
    /// <exception cref="ContextException">The task failed. The error names the seed, the floor, and the cause.</exception>
    public bool BeforeTick(SimulationLoop loop)
    {
        if (this.digging is null || !this.digging.IsCompleted || this.staged is not null)
        {
            return false;
        }

        NextFloor next = this.TakeResult();
        this.LastDigMicros = next.DigMicros;
        this.LastMeshMicros = next.MeshMicros;
        loop.OfferNextFloor(next.Plan);
        this.staged = next.Plan;
        this.stagedMeshes = next.Meshes;
        this.stagedNodes = [];
        this.stagedChunk = 0;
        return true;
    }

    /// <summary>Uploads up to <see cref="ChunksPerFrame"/> meshes of the next floor into hidden nodes. Call it once each frame.</summary>
    public void UploadSome()
    {
        if (this.staged is null)
        {
            return;
        }

        int end = Math.Min(this.stagedChunk + ChunksPerFrame, this.stagedMeshes.Count);
        for (; this.stagedChunk < end; this.stagedChunk++)
        {
            this.AddNode(this.stagedMeshes[this.stagedChunk], false, this.stagedNodes);
        }
    }

    /// <summary>The mesh data of every chunk of a grid, in chunk order: Z outer, X inner. It calls no engine API, so a task can run it.</summary>
    public static IReadOnlyList<MeshData> MeshAll(VoxelGrid grid)
    {
        List<MeshData> meshes = [];
        for (int chunkZ = 0; chunkZ < ChunkLayout.CountZ(grid); chunkZ++)
        {
            for (int chunkX = 0; chunkX < ChunkLayout.CountX(grid); chunkX++)
            {
                meshes.Add(GreedyMesher.MeshChunk(grid, chunkX, chunkZ));
            }
        }

        return meshes;
    }

    /// <summary>
    /// Swaps the nodes when the loop descended on the last tick: the old nodes go, and the nodes of the new floor
    /// show. The chunks that the upload did not reach are built in this frame. The task of the floor after it starts.
    /// </summary>
    /// <returns>True when the tick descended and the swap ran.</returns>
    public bool AfterTick(SimulationLoop loop)
    {
        if (loop.Floor == this.shownFloor)
        {
            return false;
        }

        foreach (MeshInstance3D node in this.shown)
        {
            node.QueueFree();
        }

        if (this.staged is not null && ReferenceEquals(this.staged, loop.Plan))
        {
            // The loop took the offered plan, so the hidden nodes show its grid.
            for (; this.stagedChunk < this.stagedMeshes.Count; this.stagedChunk++)
            {
                this.AddNode(this.stagedMeshes[this.stagedChunk], false, this.stagedNodes);
            }

            foreach (MeshInstance3D node in this.stagedNodes)
            {
                node.Visible = true;
            }

            this.shown = this.stagedNodes;
            this.LastSwapFromWorker = true;
        }
        else
        {
            // The descent came before the task ended, so the loop dug the floor itself, and no node holds it yet.
            // A task that is still running ends in its own time, and its result is never read.
            this.shown = this.BuildAll(loop.Grid, true);
            this.LastSwapFromWorker = false;
        }

        this.shownFloor = loop.Floor;
        this.staged = null;
        this.stagedMeshes = [];
        this.stagedNodes = [];
        this.stagedChunk = 0;
        this.digging = null;
        this.StartDig(loop.Floor + 1);
        return true;
    }

    /// <summary>Starts the task that digs one floor, when the content covers it. The stairwell of the deepest floor has none under it.</summary>
    private void StartDig(int floor)
    {
        if (!this.worker.Covers(floor))
        {
            return;
        }

        NextFloorWorker dig = this.worker;
        ulong runSeed = this.seed;
        this.diggingFloor = floor;
        this.digging = Task.Run(() =>
        {
            long digStarted = Stopwatch.GetTimestamp();
            FloorPlan plan = dig.Generate(runSeed, floor);
            long digMicros = (long)Stopwatch.GetElapsedTime(digStarted).TotalMicroseconds;
            long meshStarted = Stopwatch.GetTimestamp();
            IReadOnlyList<MeshData> meshes = MeshAll(plan.Grid);
            return new NextFloor(plan, meshes, digMicros, (long)Stopwatch.GetElapsedTime(meshStarted).TotalMicroseconds);
        });
    }

    /// <summary>The plan and the meshes of the task that ended, with the wall time of the dig and of the meshes.</summary>
    /// <exception cref="ContextException">The task failed or the engine cancelled it.</exception>
    private NextFloor TakeResult()
    {
        Task<NextFloor> task = this.digging ?? throw new InvalidOperationException(TaskFailedMessage);
        if (task.IsCompletedSuccessfully)
        {
            return task.Result;
        }

        string cause = task.Exception?.InnerException?.Message ?? task.Status.ToString();
        ContextException error = new(TaskFailedMessage);
        error.AddContext(SeedField, this.seed.ToString(CultureInfo.InvariantCulture));
        error.AddContext(FloorField, this.diggingFloor.ToString(CultureInfo.InvariantCulture));
        error.AddContext(CauseField, cause);
        throw error;
    }

    /// <summary>The nodes of every chunk of a grid that shows a face, meshed on the main thread.</summary>
    private List<MeshInstance3D> BuildAll(VoxelGrid grid, bool visible)
    {
        List<MeshInstance3D> nodes = [];
        foreach (MeshData data in MeshAll(grid))
        {
            this.AddNode(data, visible, nodes);
        }

        return nodes;
    }

    /// <summary>Builds the engine mesh of one chunk and adds its node under the parent. A chunk with no visible face gets no node.</summary>
    private void AddNode(MeshData data, bool visible, List<MeshInstance3D> nodes)
    {
        if (data.TriangleCount == 0)
        {
            return;
        }

        MeshInstance3D node = new()
        {
            Mesh = ArrayMeshBuilder.Build(data),
            MaterialOverride = this.material,
            Visible = visible,
        };
        this.parent.AddChild(node);
        nodes.Add(node);
    }
}

/// <summary>The result of the task: the plan of the next floor, the mesh data of each chunk in chunk order, and the wall time of the dig and of the meshes in microseconds.</summary>
internal sealed record NextFloor(FloorPlan Plan, IReadOnlyList<MeshData> Meshes, long DigMicros, long MeshMicros);
