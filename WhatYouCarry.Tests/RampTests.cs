using System;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The ramp cell and its block ids (D-345, D-346, D-367; PR-64).</summary>
public sealed class RampTests
{
    /// <summary>The layout of D-367 holds: the first id of each run, the rise order, and the place order. A number here never changes once a floor holds it.</summary>
    [Fact]
    public void TheRampIdsHoldTheirLayout()
    {
        Assert.Equal(8, Ramp.FirstId);
        Assert.Equal(43, Ramp.LastId);
        Assert.Equal(2, Ramp.SteepestRun);
        Assert.Equal(4, Ramp.ShallowestRun);

        Assert.Equal((BlockId)8, new Ramp(RampRise.PlusX, 2, 0).Id);
        Assert.Equal((BlockId)11, new Ramp(RampRise.MinusX, 2, 1).Id);
        Assert.Equal((BlockId)15, new Ramp(RampRise.MinusZ, 2, 1).Id);
        Assert.Equal((BlockId)16, new Ramp(RampRise.PlusX, 3, 0).Id);
        Assert.Equal((BlockId)22, new Ramp(RampRise.PlusZ, 3, 0).Id);
        Assert.Equal((BlockId)27, new Ramp(RampRise.MinusZ, 3, 2).Id);
        Assert.Equal((BlockId)28, new Ramp(RampRise.PlusX, 4, 0).Id);
        Assert.Equal((BlockId)35, new Ramp(RampRise.MinusX, 4, 3).Id);
        Assert.Equal((BlockId)43, new Ramp(RampRise.MinusZ, 4, 3).Id);
        Assert.Equal(0, (int)RampRise.PlusX);
        Assert.Equal(1, (int)RampRise.MinusX);
        Assert.Equal(2, (int)RampRise.PlusZ);
        Assert.Equal(3, (int)RampRise.MinusZ);
    }

    /// <summary>Every ramp id from 8 to 43 reads back to one ramp and writes back to its id, and no two ramps share an id.</summary>
    [Fact]
    public void EveryRampIdRoundTrips()
    {
        bool[] seen = new bool[Ramp.LastId + 1];
        for (int id = Ramp.FirstId; id <= Ramp.LastId; id++)
        {
            Assert.True(Ramp.IsRamp((BlockId)id), $"The id {id} is not a ramp.");
            Ramp ramp = Ramp.FromId((BlockId)id);
            Assert.Equal((BlockId)id, ramp.Id);
            Assert.False(seen[id], $"The id {id} names two ramps.");
            seen[id] = true;
        }

        int count = 0;
        foreach (RampRise rise in Enum.GetValues<RampRise>())
        {
            for (int run = Ramp.SteepestRun; run <= Ramp.ShallowestRun; run++)
            {
                for (int place = 0; place < run; place++)
                {
                    Assert.Equal(new Ramp(rise, run, place), Ramp.FromId(new Ramp(rise, run, place).Id));
                    count++;
                }
            }
        }

        Assert.Equal(36, count);
        Assert.False(Ramp.IsRamp(BlockId.Plank));
        Assert.False(Ramp.IsRamp((BlockId)44));
    }

    /// <summary>A ramp outside D-346, and a block id that is not a ramp, are errors that name the part at fault (T-2).</summary>
    [Fact]
    public void ABadRampIsAnError()
    {
        ContextException shortRun = Assert.Throws<ContextException>(() => new Ramp(RampRise.PlusX, 1, 0));
        Assert.Contains("run=1", shortRun.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => new Ramp(RampRise.PlusX, 5, 0));
        ContextException place = Assert.Throws<ContextException>(() => new Ramp(RampRise.PlusZ, 3, 3));
        Assert.Contains("place=3", place.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => new Ramp(RampRise.PlusZ, 3, -1));
        ContextException rise = Assert.Throws<ContextException>(() => new Ramp((RampRise)4, 2, 0));
        Assert.Contains("rise=4", rise.Message, StringComparison.Ordinal);

        ContextException plank = Assert.Throws<ContextException>(() => Ramp.FromId(BlockId.Plank));
        Assert.Contains("block=7", plank.Message, StringComparison.Ordinal);
        Assert.Throws<ContextException>(() => Ramp.FromId((BlockId)44));
    }

    /// <summary>Each place of a run holds its part of one block of rise, so the places meet with no step, and the last place meets the next row.</summary>
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void TheSlopeRisesOneBlockOverTheRun(int run)
    {
        for (int place = 0; place < run; place++)
        {
            Ramp ramp = new(RampRise.PlusX, run, place);
            Assert.Equal(5.0f + (place / (float)run), ramp.SlopeAt(5, 0.0f));
            Assert.Equal(5.0f + ((place + 1) / (float)run), ramp.SlopeAt(5, 1.0f));
            Assert.Equal(0.0f, ramp.MeetAt(5, ramp.SlopeAt(5, 0.0f)), 5);
            Assert.Equal(0.5f, ramp.MeetAt(5, ramp.SlopeAt(5, 0.5f)), 5);
            if (place > 0)
            {
                Assert.Equal(new Ramp(RampRise.PlusX, run, place - 1).SlopeAt(5, 1.0f), ramp.SlopeAt(5, 0.0f));
            }
        }

        Assert.Equal(6.0f, new Ramp(RampRise.PlusX, run, run - 1).SlopeAt(5, 1.0f));
    }

    /// <summary>The distance along the rise runs from the low face to the high face in each direction, and the highest point under a footprint lies at its edge nearest the high face.</summary>
    [Fact]
    public void TheHighestPointLiesTowardTheHighFace()
    {
        Assert.Equal(0.25f, new Ramp(RampRise.PlusX, 2, 0).Along(3, 7, 3.25f, 7.9f));
        Assert.Equal(0.75f, new Ramp(RampRise.MinusX, 2, 0).Along(3, 7, 3.25f, 7.9f));
        Assert.Equal(0.5f, new Ramp(RampRise.PlusZ, 2, 0).Along(3, 7, 3.9f, 7.5f));
        Assert.Equal(0.25f, new Ramp(RampRise.MinusZ, 2, 0).Along(3, 7, 3.9f, 7.75f));

        // A footprint from 3.2 to 3.8 on X and from 6.9 to 7.5 on Z over the cell (3, 1, 7).
        Assert.Equal(1.4f, new Ramp(RampRise.PlusX, 2, 0).HighestUnder(3, 1, 7, 3.2f, 3.8f, 6.9f, 7.5f), 5);
        Assert.Equal(1.4f, new Ramp(RampRise.MinusX, 2, 0).HighestUnder(3, 1, 7, 3.2f, 3.8f, 6.9f, 7.5f), 5);
        Assert.Equal(1.25f, new Ramp(RampRise.PlusZ, 2, 0).HighestUnder(3, 1, 7, 3.2f, 3.8f, 6.9f, 7.5f), 5);
        Assert.Equal(1.5f, new Ramp(RampRise.MinusZ, 2, 0).HighestUnder(3, 1, 7, 3.2f, 3.8f, 6.9f, 7.5f), 5);

        // A footprint that reaches past the high face gives the top of the cell, exact in float.
        Assert.Equal(2.0f, new Ramp(RampRise.PlusX, 2, 1).HighestUnder(3, 1, 7, 3.5f, 4.1f, 7.2f, 7.8f));
        Assert.Equal(-0.25f, new Ramp(RampRise.PlusX, 2, 0).HeightOver(3, 1, 7, 3.5f, 1.0f, 7.5f));
    }

    /// <summary>The grid takes every ramp id, gives the ramp back, reads a ramp as solid, and holds no ramp outside the grid or in a block (D-237, D-367).</summary>
    [Fact]
    public void TheGridHoldsRamps()
    {
        VoxelGrid grid = new(2, 2, 2);
        for (int id = Ramp.FirstId; id <= Ramp.LastId; id++)
        {
            grid.Set(1, 1, 1, (BlockId)id);
            Assert.Equal((BlockId)id, grid.Get(1, 1, 1));
            Assert.True(grid.TryGetRamp(1, 1, 1, out Ramp ramp));
            Assert.Equal((BlockId)id, ramp.Id);
            Assert.True(grid.IsSolid(1, 1, 1));
            Assert.True(VoxelGrid.IsSolidBlock((BlockId)id));
        }

        grid.Set(0, 0, 0, BlockId.RawStone);
        Assert.False(grid.TryGetRamp(0, 0, 0, out _));
        Assert.False(grid.TryGetRamp(0, 1, 0, out _));
        Assert.False(grid.TryGetRamp(-1, 1, 1, out _));
        Assert.False(grid.TryGetRamp(1, 2, 1, out _));
    }

    /// <summary>The top under a footprint reads the top of a block, the highest point of a slope, the outside as rock, and an open row as nothing.</summary>
    [Fact]
    public void TheTopUnderAFootprintReadsBlocksAndSlopes()
    {
        VoxelGrid grid = new(4, 4, 4);
        grid.Set(1, 1, 1, BlockId.RawStone);
        grid.Set(2, 1, 1, new Ramp(RampRise.PlusX, 2, 1).Id);

        Assert.True(grid.TryTopUnder(1, 2.1f, 2.7f, 1.2f, 1.8f, out float slope));
        Assert.Equal(1.85f, slope, 5);
        Assert.True(grid.TryTopUnder(1, 1.7f, 2.3f, 1.2f, 1.8f, out float mixed));
        Assert.Equal(2.0f, mixed);
        Assert.False(grid.TryTopUnder(2, 1.7f, 2.3f, 1.2f, 1.8f, out _));
        Assert.True(grid.TryTopUnder(-1, 1.7f, 2.3f, 1.2f, 1.8f, out float outside));
        Assert.Equal(0.0f, outside);
    }
}
