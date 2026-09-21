using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using WhatYouCarry.Core.Bots;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Simulation;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The stairwell prompt, the next floor worker, the ascend, and the coward (D-72, D-140, D-429 to D-433; PR-18 exit tests 1 to 5).</summary>
public sealed class TransitionTests
{
    /// <summary>The count of ticks that a walk may take before the test fails.</summary>
    private const int MaxWalkTicks = 20000;

    /// <summary>An intent that does nothing on one tick.</summary>
    private static Intent Idle(SimulationLoop loop) => new(loop.Tick, 0, 0, 0, 0, 0);

    /// <summary>Steps the loop with the greedy descender until the prompt opens on floor 1. The run must not end or leave the floor first.</summary>
    private static void WalkToPrompt(SimulationLoop loop, IBotPolicy policy)
    {
        while (!StairwellPrompt.IsOpen(loop))
        {
            Assert.True(loop.Floor == SimulationLoop.FirstFloor && !loop.Ended && loop.Tick < MaxWalkTicks, $"Seed {loop.Seed}: the walk left floor 1, ended, or passed its ticks at tick {loop.Tick}.");
            loop.Step(policy.Next(loop));
        }
    }

    /// <summary>
    /// PR-18 exit test 1. The worker on a task gives the grid, the spawn, and the stairwell of a dig on the calling
    /// thread, for the same seed and floor (D-72, D-429). Over ten seeds and every floor below the first.
    /// </summary>
    [Fact]
    public async Task WorkerEqualsSynchronous()
    {
        NextFloorWorker worker = new(TestWorld.Content);
        for (ulong seed = 1; seed <= 10; seed++)
        {
            List<Task<FloorPlan>> tasks = [];
            for (int floor = 2; floor <= 15; floor++)
            {
                ulong runSeed = seed;
                int next = floor;
                tasks.Add(Task.Run(() => worker.Generate(runSeed, next)));
            }

            for (int index = 0; index < tasks.Count; index++)
            {
                int floor = index + 2;
                FloorPlan onTask = await tasks[index];
                FloorPlan onThread = FloorGenerator.Generate(seed, floor, TestWorld.Content);
                Assert.True(ProcgenTests.GridHash(onTask.Grid).Value == ProcgenTests.GridHash(onThread.Grid).Value, $"Seed {seed}, floor {floor}: the worker grid differs from the synchronous grid.");
                Assert.Equal(onThread.Spawn, onTask.Spawn);
                Assert.Equal(onThread.Stairwell, onTask.Stairwell);
                Assert.Equal(floor, onTask.Floor);
            }
        }
    }

    /// <summary>
    /// A run whose loop takes each next floor from the worker keeps the state hash of a run that digs at the descent,
    /// on every tick through two descents (D-429). The worker changes no tick.
    /// </summary>
    [Fact]
    public async Task OfferedFloorKeepsTheRunHash()
    {
        const ulong seed = 3;
        NextFloorWorker worker = new(TestWorld.PeacefulContent);
        SimulationLoop offered = TestWorld.NewLoop(seed);
        SimulationLoop dug = TestWorld.NewLoop(seed);
        GreedyDescender offeredPolicy = new(TestWorld.PeacefulContent);
        GreedyDescender dugPolicy = new(TestWorld.PeacefulContent);
        Task<FloorPlan> next = Task.Run(() => worker.Generate(seed, 2));
        bool onOffer = false;
        while (offered.Floor < 3)
        {
            Assert.True(offered.Tick < 3 * MaxWalkTicks && !offered.Ended, $"The run ended or passed its ticks on floor {offered.Floor} at tick {offered.Tick}.");
            // The worker gives its plan when it ends, and at the latest when the prompt opens, the tick before any
            // descent, so the offer never depends on the speed of the machine.
            if (!onOffer && (next.IsCompleted || StairwellPrompt.IsOpen(offered)))
            {
                offered.OfferNextFloor(await next);
                onOffer = true;
            }

            int floor = offered.Floor;
            offered.Step(offeredPolicy.Next(offered));
            dug.Step(dugPolicy.Next(dug));
            Assert.True(offered.Hash().Value == dug.Hash().Value, $"Tick {offered.Tick}: the run with the offered floor left the run that digs.");
            if (offered.Floor != floor)
            {
                Assert.True(onOffer, $"The descent to floor {offered.Floor} came before the worker ended, so the test proves nothing of the offer.");
                int deeper = offered.Floor + 1;
                next = Task.Run(() => worker.Generate(seed, deeper));
                onOffer = false;
            }
        }
    }

    /// <summary>An offer of a plan that is not for the next floor is an error that names both floors (T-2).</summary>
    [Fact]
    public void OfferRejectsAnotherFloor()
    {
        SimulationLoop loop = TestWorld.NewLoop(1);
        NextFloorWorker worker = new(TestWorld.PeacefulContent);
        ContextException error = Assert.Throws<ContextException>(() => loop.OfferNextFloor(worker.Generate(1, 3)));
        Assert.Contains("floor 3", error.Message, StringComparison.Ordinal);
        Assert.True(worker.Covers(15));
        Assert.False(worker.Covers(16));
        Assert.False(worker.Covers(0));
    }

    /// <summary>
    /// PR-18 exit test 2. The worker reads the seed and the floor number alone (D-429): its one dig method takes a
    /// seed and a floor, and it holds the content set and nothing more, so no path reaches the state of a loop.
    /// </summary>
    [Fact]
    public void WorkerReadsNoSimulationState()
    {
        Type worker = typeof(NextFloorWorker);
        MethodInfo generate = worker.GetMethod(nameof(NextFloorWorker.Generate)) ?? throw new InvalidOperationException("The worker has no dig method.");
        ParameterInfo[] parameters = generate.GetParameters();
        Assert.Equal([typeof(ulong), typeof(int)], Array.ConvertAll(parameters, parameter => parameter.ParameterType));
        Assert.Equal(typeof(FloorPlan), generate.ReturnType);

        foreach (ConstructorInfo constructor in worker.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            Assert.Equal([typeof(ContentSet)], Array.ConvertAll(constructor.GetParameters(), parameter => parameter.ParameterType));
        }

        FieldInfo[] fields = worker.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal([typeof(ContentSet)], Array.ConvertAll(fields, field => field.FieldType));
        Assert.True(fields[0].IsInitOnly, "The content field of the worker can change after the construction.");
    }

    /// <summary>
    /// PR-18 exit test 3. The prompt opens when the body comes to the stairwell cell, the countdown holds on each tick
    /// that it is open, and the countdown runs again when the body leaves the cell and the prompt closes (D-140,
    /// D-431, D-432).
    /// </summary>
    [Fact]
    public void PromptIsUntimed()
    {
        SimulationLoop loop = TestWorld.NewLoop(3UL);
        WalkToPrompt(loop, new GreedyDescender(TestWorld.PeacefulContent));
        Assert.True(StairwellTransition.IsAtStairwell(loop.Body, loop.Plan.Stairwell));

        long remaining = loop.Timer.Remaining;
        Assert.True(remaining > 0);
        for (int tick = 0; tick < 600; tick++)
        {
            loop.Step(Idle(loop));
            Assert.True(StairwellPrompt.IsOpen(loop), $"The prompt closed on tick {loop.Tick} with the body still.");
            Assert.Equal(remaining, loop.Timer.Remaining);
        }

        // The prompt holds no one (D-431): a walk takes the body off the cell, and the countdown runs again.
        int walked = 0;
        while (StairwellPrompt.IsOpen(loop))
        {
            Assert.True(walked < 600, "The body did not leave the stairwell cell in ten seconds of walk.");
            loop.Step(new Intent(loop.Tick, 0, 0, 0, 127, 0));
            walked++;
        }

        long offCell = loop.Timer.Remaining;
        loop.Step(Idle(loop));
        Assert.False(StairwellPrompt.IsOpen(loop), "The prompt opened again with the body still off the cell.");
        Assert.Equal(offCell - 1, loop.Timer.Remaining);
    }

    /// <summary>PR-18 exit test 4. The ascend bit at the stairwell ends the run as an ascend, with no next floor and no later tick (D-50, D-257).</summary>
    [Fact]
    public void AscendEndsRun()
    {
        SimulationLoop loop = TestWorld.NewLoop(3UL);
        WalkToPrompt(loop, new GreedyDescender(TestWorld.PeacefulContent));
        loop.Step(new Intent(loop.Tick, 0, 0, 0, 0, Button.Ascend));

        Assert.Equal(RunEnd.Ascend, loop.End);
        Assert.Equal(SimulationLoop.FirstFloor, loop.Floor);
        Assert.False(StairwellPrompt.IsOpen(loop), "The prompt stays open after the run ended.");
        Assert.Throws<ContextException>(() => loop.Step(Idle(loop)));
        Assert.Throws<ContextException>(() => loop.OfferNextFloor(FloorGenerator.Generate(3UL, 2, TestWorld.PeacefulContent)));
    }

    /// <summary>
    /// PR-18 exit test 5. The coward ascends at the first stairwell, and its run ends as an ascend on floor 1, the
    /// end state of D-430. A death stays a real outcome of a fight (D-403), and no run ends another way. Over fifty
    /// seeds of the full content.
    /// </summary>
    [Fact]
    public void CowardAscendsFirst()
    {
        Coward coward = new();
        Assert.True(coward.PromisesProgress, "The coward promises progress (D-433).");
        int ascends = 0;
        for (ulong seed = 1; seed <= 50; seed++)
        {
            BotRunResult result = BotRun.Play(new Coward(), seed, TestWorld.Content);
            Assert.True(
                result.End == BotRunEnd.Ascend || result.End == BotRunEnd.Death,
                $"Seed {seed}: the coward ended as {result.End} on floor {result.FloorsReached} after {result.Ticks} ticks. {result.Error}");
            Assert.Equal(SimulationLoop.FirstFloor, result.FloorsReached);
            if (result.End == BotRunEnd.Ascend)
            {
                ascends++;
            }
        }

        Assert.True(ascends > 25, $"The coward ascended on {ascends} of fifty seeds, so most runs never reach the first stairwell.");
    }

    /// <summary>An ascend on the deepest floor stays the bottom, and an ascend above it is the sixth end state (D-270, D-430).</summary>
    [Fact]
    public void AscendAboveTheLastFloorIsNotTheBottom()
    {
        Assert.Equal(15, FloorGenerator.DeepestFloor(TestWorld.Content));
        BotRunResult coward = BotRun.Play(new Coward(), 3UL, TestWorld.PeacefulContent);
        Assert.Equal(BotRunEnd.Ascend, coward.End);
        Assert.Equal("ascend", WhatYouCarry.Tools.BotRunner.BotRunCommand.EndStateText(BotRunEnd.Ascend));
        Assert.Equal("bottom", WhatYouCarry.Tools.BotRunner.BotRunCommand.EndStateText(BotRunEnd.Bottom));
    }
}
