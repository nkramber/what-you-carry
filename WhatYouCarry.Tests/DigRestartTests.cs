using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The dig restart (D-359 to D-361, PR-67 exit test 1). A dig that runs 1000 jobs with a chamber still in rock starts
/// again, with a new chamber draw from the next draws of the stream, and a floor with no complete dig in four digs is an
/// error. Before PR-67 a dig ran to the job cap of 10000, and the 7 floors of F-98 ended in an error there.
/// </summary>
public sealed class DigRestartTests
{
    /// <summary>The 7 floors of F-98: each ran the 10000 jobs of the old cap with a chamber still in rock.</summary>
    public static readonly (ulong Seed, int Floor)[] TailFloors =
    [
        (193207UL, 8),
        (286534UL, 5),
        (370718UL, 9),
        (451689UL, 10),
        (469501UL, 2),
        (514012UL, 8),
        (573862UL, 8),
    ];

    /// <summary>
    /// The three floors of the largest need in the floors that the night digs, measured 2026-09-14: 5890, 5662, and 5568
    /// jobs under the old cap. Each dug under that cap, and each now runs out of the budget and digs again.
    /// </summary>
    public static readonly (ulong Seed, int Floor)[] HeavyNightFloors =
    [
        (43783UL, 14),
        (62900UL, 6),
        (1365UL, 13),
    ];

    /// <summary>PR-67 exit test 1. Each floor of F-98 digs every chamber of its budget on its second dig, and the generator gives that dig.</summary>
    [Fact]
    public void EveryTailFloorDigs()
    {
        foreach ((ulong seed, int floor) in TailFloors)
        {
            FloorPlan plan = FloorGenerator.Generate(seed, floor, TestWorld.Content);
            Replay replay = ReplayDigs(seed, floor);
            Assert.True(replay.Digs == 2, $"Seed {seed}, floor {floor}: every chamber came on dig {replay.Digs}, and the measurement of 2026-09-14 found it on dig 2.");
            AssertSameChambers(seed, floor, replay.Plan, plan);
        }
    }

    /// <summary>A heavy floor of the night runs its first dig to the budget with a chamber in rock, and its second dig digs every chamber (D-359).</summary>
    [Fact]
    public void TheBudgetEndsTheFirstDigOfAHeavyFloor()
    {
        foreach ((ulong seed, int floor) in HeavyNightFloors)
        {
            Replay replay = ReplayDigs(seed, floor);
            Assert.True(replay.FirstDigJobs == DigPlan.JobBudget, $"Seed {seed}, floor {floor}: the first dig ran {replay.FirstDigJobs} jobs, and the budget is {DigPlan.JobBudget}.");
            Assert.True(replay.Digs == 2, $"Seed {seed}, floor {floor}: every chamber came on dig {replay.Digs}.");
            AssertSameChambers(seed, floor, replay.Plan, FloorGenerator.Generate(seed, floor, TestWorld.Content));
        }
    }

    /// <summary>A floor inside the budget digs once, and the generator gives that dig.</summary>
    [Fact]
    public void AFloorInsideTheBudgetDigsOnce()
    {
        Replay replay = ReplayDigs(1UL, 1);
        Assert.True(replay.Digs == 1, $"Seed 1, floor 1: every chamber came on dig {replay.Digs}.");
        Assert.True(replay.FirstDigJobs < DigPlan.JobBudget, $"Seed 1, floor 1: the first dig ran {replay.FirstDigJobs} jobs.");
        AssertSameChambers(1UL, 1, replay.Plan, FloorGenerator.Generate(1UL, 1, TestWorld.Content));
    }

    /// <summary>
    /// A floor with no complete dig in four digs is an error that names the digs, the budget, and the chambers (D-360, T-2).
    /// A tunnel width of 63 on a floor 64 blocks wide puts every stamp past the shell, so no walker takes a step and no
    /// chamber after the first is dug. The validator rejects such a width (D-352), and this template never meets it.
    /// </summary>
    [Fact]
    public void AFloorWithNoCompleteDigIsAnError()
    {
        FloorTemplate blocked = FloorGenerator.TemplateFor(1, TestWorld.Content) with { Id = "blocked", GalleryWidth = 63, DriftWidth = 63 };
        List<FloorTemplate> floors = [blocked];
        ContentSet content = TestWorld.Content with { Floors = floors };
        ContextException error = Assert.Throws<ContextException>(() => FloorGenerator.Generate(1UL, 1, content));
        Assert.Contains($"digs={FloorGenerator.MaxDigs}", error.Message, StringComparison.Ordinal);
        Assert.Contains($"jobBudget={DigPlan.JobBudget}", error.Message, StringComparison.Ordinal);
        Assert.Contains("chambersDug=1", error.Message, StringComparison.Ordinal);
        Assert.Contains("seed=1", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The digs of one floor up to the dig that digs every chamber: the count of digs, the jobs of the first dig, and the plan of the last dig.</summary>
    private readonly record struct Replay(int Digs, int FirstDigJobs, DigPlan Plan);

    /// <summary>Runs the digs of one floor in the order of the floor generator, before the shafts and the detail, up to the dig that digs every chamber of its draw.</summary>
    /// <exception cref="InvalidOperationException">No dig of <see cref="FloorGenerator.MaxDigs"/> digs every chamber.</exception>
    private static Replay ReplayDigs(ulong seed, int floor)
    {
        ContentSet content = TestWorld.Content;
        FloorTemplate template = FloorGenerator.TemplateFor(floor, content);
        Rng rng = Rng.ForStream(seed, RngStream.Procgen, floor);
        int firstDigJobs = 0;
        for (int dig = 1; dig <= FloorGenerator.MaxDigs; dig++)
        {
            IReadOnlyList<ChamberKind> kinds = ChamberBudget.Draw(rng, template, content.Chambers);
            DigPlan plan = new(rng, new DigCanvas(new VoxelGrid(template.SizeX, template.SizeY, template.SizeZ)), template, kinds);
            plan.DigFirstChamber();
            bool complete = plan.TryDigUntilComplete(out int jobs);
            if (dig == 1)
            {
                firstDigJobs = jobs;
            }

            if (complete)
            {
                Assert.True(plan.Chambers.Count == kinds.Count, $"Seed {seed}, floor {floor}: dig {dig} dug {plan.Chambers.Count} of {kinds.Count} chambers.");
                return new Replay(dig, firstDigJobs, plan);
            }
        }

        throw new InvalidOperationException($"Seed {seed}, floor {floor}: no dig of {FloorGenerator.MaxDigs} dug every chamber.");
    }

    /// <summary>Asserts that the generator gives the chambers of the replayed dig: the same count, and each chamber with the same kind, anchor, and floor row.</summary>
    private static void AssertSameChambers(ulong seed, int floor, DigPlan replayed, FloorPlan generated)
    {
        Assert.True(generated.Chambers.Count == replayed.Chambers.Count, $"Seed {seed}, floor {floor}: the generator gives {generated.Chambers.Count} chambers, and the replay dug {replayed.Chambers.Count}.");
        for (int index = 0; index < replayed.Chambers.Count; index++)
        {
            Chamber expected = replayed.Chambers[index];
            Chamber actual = generated.Chambers[index];
            bool same = expected.Kind.Id == actual.Kind.Id && expected.Anchor.Equals(actual.Anchor) && expected.FloorRow == actual.FloorRow;
            Assert.True(same, $"Seed {seed}, floor {floor}: chamber {index} of the generator differs from the replay.");
        }
    }
}
