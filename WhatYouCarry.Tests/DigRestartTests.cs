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
    /// The floors whose first dig runs the job budget with a chamber still in rock, measured 2026-09-20 over the
    /// first 20000 seeds of the PR-66 dig. About one floor in 3300 needs a second dig.
    /// </summary>
    /// <remarks>
    /// The three heavy night floors of 2026-09-14, seed 43783 floor 14, seed 62900 floor 6, and seed 1365 floor 13,
    /// held this place before PR-66. The ramps of D-347 and the tiers of D-348 changed every floor, so each of those
    /// three digs inside the budget now, and this list replaces them.
    /// </remarks>
    public static readonly (ulong Seed, int Floor)[] BudgetFloors =
    [
        (1818UL, 4),
        (2327UL, 3),
        (4839UL, 10),
    ];

    /// <summary>
    /// PR-67 exit test 1. Each floor of F-98 digs every chamber of its budget, and the generator gives that dig.
    /// Before PR-67 each of the seven ran the job cap of 10000 with a chamber in rock, and the floor was an error.
    /// </summary>
    /// <remarks>
    /// The measurement of 2026-09-14 found every chamber on the second dig. The dig of PR-66 changed every floor,
    /// and each of the seven now digs every chamber on its first dig. The guarantee that this test holds is the
    /// complete floor, and <see cref="TheBudgetEndsTheFirstDigOfAHeavyFloor"/> reads the restart itself.
    /// </remarks>
    [Fact]
    public void EveryTailFloorDigs()
    {
        foreach ((ulong seed, int floor) in TailFloors)
        {
            FloorPlan plan = FloorGenerator.Generate(seed, floor, TestWorld.Content);
            Replay replay = ReplayDigs(seed, floor);
            Assert.True(replay.Digs <= FloorGenerator.MaxDigs, $"Seed {seed}, floor {floor}: every chamber came on dig {replay.Digs}.");
            AssertSameChambers(seed, floor, replay.Plan, plan);
        }
    }

    /// <summary>A floor of <see cref="BudgetFloors"/> runs its first dig to the budget with a chamber in rock, and its second dig digs every chamber (D-359).</summary>
    [Fact]
    public void TheBudgetEndsTheFirstDigOfAHeavyFloor()
    {
        foreach ((ulong seed, int floor) in BudgetFloors)
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
