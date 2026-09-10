using System.Collections.Generic;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Determinism;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>
/// The job cap of the dig plan holds the measured tail (D-279, F-92). The first night found three floors that
/// needed more than the 400 jobs of the first cap, and each crashed the run. This test digs those floors and
/// fails on the old cap.
/// </summary>
public sealed class DigPlanJobCapTests
{
    /// <summary>The three floors of the first night that passed 400 jobs, with the job count each needed on 2026-09-10.</summary>
    public static readonly (ulong Seed, int Floor, int Jobs)[] TailFloors =
    [
        (2170UL, 10, 2817),
        (3000UL, 4, 1661),
        (4786UL, 10, 615),
    ];

    /// <summary>Each tail floor digs every chamber of its budget, needs more jobs than the first cap of 400, and stays under the cap.</summary>
    [Fact]
    public void TheCapHoldsTheMeasuredTail()
    {
        foreach ((ulong seed, int floor, int measured) in TailFloors)
        {
            int jobs = JobsToDig(seed, floor, out int chambers, out int needed);
            Assert.True(chambers == needed, $"Seed {seed}, floor {floor}: dug {chambers} of {needed} chambers.");
            Assert.True(jobs > 400, $"Seed {seed}, floor {floor}: {jobs} jobs, and the first night needed {measured}.");
            Assert.True(jobs < DigPlan.MaxJobs, $"Seed {seed}, floor {floor}: {jobs} jobs, and the cap is {DigPlan.MaxJobs}.");
            Assert.Equal(measured, jobs);
        }
    }

    /// <summary>The whole generator, with the detail and the stairwell, accepts each tail floor.</summary>
    [Fact]
    public void TheGeneratorAcceptsEachTailFloor()
    {
        foreach ((ulong seed, int floor, _) in TailFloors)
        {
            FloorPlan plan = FloorGenerator.Generate(seed, floor, TestWorld.Content);
            Assert.True(plan.Chambers.Count > 0, $"Seed {seed}, floor {floor}: no chamber.");
        }
    }

    /// <summary>The dig alone, as <see cref="FloorGenerator"/> runs it before the shafts and the detail, with the job count.</summary>
    private static int JobsToDig(ulong seed, int floor, out int chambers, out int needed)
    {
        ContentSet content = TestWorld.Content;
        FloorTemplate template = FloorGenerator.TemplateFor(floor, content);
        Rng rng = Rng.ForStream(seed, RngStream.Procgen, floor);
        IReadOnlyList<ChamberKind> kinds = ChamberBudget.Draw(rng, template, content.Chambers);
        VoxelGrid grid = new(template.SizeX, template.SizeY, template.SizeZ);
        DigCanvas canvas = new(grid);
        DigPlan plan = new(rng, canvas, kinds);
        plan.DigFirstChamber();
        int jobs = plan.DigUntilComplete();
        chambers = plan.Chambers.Count;
        needed = kinds.Count;
        return jobs;
    }
}
