using System.Collections.Generic;
using System.Globalization;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Core.Content;

/// <summary>
/// One floor template (D-6, D-46, D-166, D-167, D-210, D-252, D-341 to D-344, D-407, D-410). The floor generator reads one to dig a floor, and the loop reads its timer and its waves.
/// </summary>
/// <remarks>
/// <para>
/// The size of the floor is in blocks (D-252, D-343). The validator rejects a size past the grid limit of D-164, and a
/// size below the smallest floor that the dig plan can carve.
/// </para>
/// <para>
/// The dig sizes name the gallery, the drifts, and the chamber heights (D-341, D-342). Each is at least the three blocks
/// of D-166. A tunnel width is odd, because the brush of a walker is centered on the cell it stands on, and it fits
/// inside the rock shell of the floor. A dig height leaves three rows of the floor as rock and floor: the base row, one
/// floor row, and the top row (D-352).
/// </para>
/// <para>
/// The timer of a floor of the band runs <c>timerSeconds</c>, and a boss floor of D-6 runs <c>bossTimerSeconds</c>
/// more (D-407). After expiry, a wave spawns each <c>waveIntervalSeconds</c>, up to <c>waveCap</c> living wave
/// enemies (D-410, D-424). The loop counts in ticks, so each count of seconds is a whole number.
/// </para>
/// </remarks>
public sealed record FloorTemplate(string Id, long MinDepth, long MaxDepth, long RoomCountMin, long RoomCountMax, long DifficultyBudget, string Band, int SizeX, int SizeY, int SizeZ, int GalleryWidth, int GalleryHeight, int DriftWidth, int DriftHeight, int ChamberHeightMin, int ChamberHeightMax, IReadOnlyList<int> RampSlopeRuns, long TimerSeconds, long BossTimerSeconds, long WaveIntervalSeconds, long WaveCap)
{
    /// <summary>The smallest size along X or Z that the dig plan can carve, in blocks.</summary>
    public const int MinSizeXZ = 24;

    /// <summary>The smallest dig size, in blocks: the tunnel cross-section of D-166.</summary>
    public const int SmallestDigSize = 3;

    /// <summary>The rows of a floor that no dig height takes: the base row of the shell, one floor row, and the top row of the shell.</summary>
    public const int RowsOutsideDig = 3;

    /// <summary>The columns of a floor that no tunnel width takes on one axis: the shell on each side.</summary>
    public const int ColumnsOutsideDig = 2;

    /// <summary>The smallest size along Y that the dig plan can carve, in blocks: the smallest dig height and the rows outside it.</summary>
    public const int MinSizeY = SmallestDigSize + RowsOutsideDig;

    /// <summary>
    /// The largest count of seconds of a timer or a wave interval. The loop multiplies the count by the ticks per second
    /// into a long, and a larger count wraps the product, to a timer of one second among others (F-120).
    /// </summary>
    public const long LargestSeconds = ContentValidator.LargestLong / Simulation.SimulationLoop.TicksPerSecond;

    /// <summary>The band of the working mine, floors 1 to 5 (D-210).</summary>
    public const string WorkingMineBand = "working-mine";

    /// <summary>The band of the older workings, floors 6 to 10 (D-210).</summary>
    public const string OlderWorkingsBand = "older-workings";

    /// <summary>The deep band, floors 11 to 15 (D-210).</summary>
    public const string DeepBand = "what-the-miners-reached";

    /// <summary>The three bands of D-210. The loader checks the band against them, so an unknown band fails at load and not when a floor of it is dug (G-7).</summary>
    public static readonly IReadOnlyList<string> Bands = [WorkingMineBand, OlderWorkingsBand, DeepBand];

    /// <summary>The names that a floor template must carry.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "id",
        "minDepth",
        "maxDepth",
        "roomCountMin",
        "roomCountMax",
        "difficultyBudget",
        "band",
        "sizeX",
        "sizeY",
        "sizeZ",
        "galleryWidth",
        "galleryHeight",
        "driftWidth",
        "driftHeight",
        "chamberHeightMin",
        "chamberHeightMax",
        "rampSlopeRuns",
        "timerSeconds",
        "bossTimerSeconds",
        "waveIntervalSeconds",
        "waveCap",
    ];

    /// <summary>The names that a floor template can carry.</summary>
    public static readonly IReadOnlyList<string> Optional = [];

    /// <summary>One template from one validated object.</summary>
    /// <exception cref="Logging.ContextException">A field is absent, unknown, or of another kind, or a value is outside its bounds.</exception>
    public static FloorTemplate FromMembers(string path, IReadOnlyList<JsonMember> members)
    {
        ContentValidator.Check(path, members, Required, Optional);

        long minDepth = Number(path, members, "minDepth");
        long maxDepth = Number(path, members, "maxDepth");
        long roomMin = Number(path, members, "roomCountMin");
        long roomMax = Number(path, members, "roomCountMax");
        long budget = Number(path, members, "difficultyBudget");

        if (minDepth > maxDepth)
        {
            throw ContentError.Make(path, "minDepth", "is above maxDepth, and a band covers no floor then");
        }

        // A floor number is an int, and a cast of a larger depth wraps the deepest floor of the content (F-120).
        if (maxDepth > ContentValidator.LargestInt)
        {
            throw ContentError.Make(path, "maxDepth", $"is {maxDepth}, and a floor number is an int of no more than {ContentValidator.LargestInt}");
        }

        if (roomMin > roomMax)
        {
            throw ContentError.Make(path, "roomCountMin", "is above roomCountMax, and no room count fits then");
        }

        if (roomMin < 1)
        {
            throw ContentError.Make(path, "roomCountMin", "is below one, and every floor holds a room");
        }

        if (budget < 10)
        {
            throw ContentError.Make(path, "difficultyBudget", "is below ten, and the budget window of D-167 is one tenth of the budget");
        }

        int sizeX = Size(path, members, "sizeX", MinSizeXZ, VoxelGrid.MaxSizeX);
        int sizeY = Size(path, members, "sizeY", MinSizeY, VoxelGrid.MaxSizeY);
        int sizeZ = Size(path, members, "sizeZ", MinSizeXZ, VoxelGrid.MaxSizeZ);

        int galleryWidth = TunnelWidth(path, members, "galleryWidth", sizeX, sizeZ);
        int galleryHeight = DigHeight(path, members, "galleryHeight", sizeY);
        int driftWidth = TunnelWidth(path, members, "driftWidth", sizeX, sizeZ);
        int driftHeight = DigHeight(path, members, "driftHeight", sizeY);
        int chamberHeightMin = DigHeight(path, members, "chamberHeightMin", sizeY);
        int chamberHeightMax = DigHeight(path, members, "chamberHeightMax", sizeY);
        if (chamberHeightMin > chamberHeightMax)
        {
            throw ContentError.Make(path, "chamberHeightMin", "is above chamberHeightMax, and no chamber height fits then");
        }

        IReadOnlyList<int> rampSlopeRuns = RampSlopes(path, members);

        long timerSeconds = Number(path, members, "timerSeconds");
        if (timerSeconds < 1)
        {
            throw ContentError.Make(path, "timerSeconds", $"is {timerSeconds}, and every floor has a time limit of one second or more (D-44, D-407)");
        }

        if (timerSeconds > LargestSeconds)
        {
            throw ContentError.Make(path, "timerSeconds", $"is {timerSeconds}, and the ticks of the timer are a long, so the timer is no more than {LargestSeconds} seconds");
        }

        long bossTimerSeconds = Number(path, members, "bossTimerSeconds");
        if (bossTimerSeconds < 0)
        {
            throw ContentError.Make(path, "bossTimerSeconds", $"is {bossTimerSeconds}, and a boss floor gets zero or more extra seconds (D-140, D-407)");
        }

        // A boss floor runs both counts, so their sum, and not each count alone, fits the long of the ticks.
        if (bossTimerSeconds > LargestSeconds - timerSeconds)
        {
            throw ContentError.Make(path, "bossTimerSeconds", $"is {bossTimerSeconds}, and the ticks of a boss floor timer are a long, so the timer of {timerSeconds} seconds takes no more than {LargestSeconds - timerSeconds} extra seconds");
        }

        long waveIntervalSeconds = Number(path, members, "waveIntervalSeconds");
        if (waveIntervalSeconds < 1)
        {
            throw ContentError.Make(path, "waveIntervalSeconds", $"is {waveIntervalSeconds}, and two waves cannot spawn on one tick (D-410)");
        }

        if (waveIntervalSeconds > LargestSeconds)
        {
            throw ContentError.Make(path, "waveIntervalSeconds", $"is {waveIntervalSeconds}, and the ticks of the interval are a long, so the interval is no more than {LargestSeconds} seconds");
        }

        long waveCap = Number(path, members, "waveCap");
        if (waveCap < 0)
        {
            throw ContentError.Make(path, "waveCap", $"is {waveCap}, and the cap of living wave enemies is zero or more (D-410)");
        }

        string band = ContentValidator.Value(path, members, "band", JsonMemberKind.Text);
        bool knownBand = false;
        foreach (string known in Bands)
        {
            if (band == known)
            {
                knownBand = true;
            }
        }

        if (!knownBand)
        {
            throw ContentError.Make(path, "band", $"is '{band}', and a band is one of the three bands of D-210: '{WorkingMineBand}', '{OlderWorkingsBand}', or '{DeepBand}'");
        }

        return new FloorTemplate(
            ContentValidator.Value(path, members, "id", JsonMemberKind.Text),
            minDepth,
            maxDepth,
            roomMin,
            roomMax,
            budget,
            band,
            sizeX,
            sizeY,
            sizeZ,
            galleryWidth,
            galleryHeight,
            driftWidth,
            driftHeight,
            chamberHeightMin,
            chamberHeightMax,
            rampSlopeRuns,
            timerSeconds,
            bossTimerSeconds,
            waveIntervalSeconds,
            waveCap);
    }

    /// <summary>
    /// The ramp slope runs of the band, in file order (D-346). Each run is from the steepest to the shallowest
    /// of <see cref="Ramp"/>, and no run appears twice.
    /// </summary>
    /// <exception cref="Logging.ContextException">The field holds no list of numbers, the list is empty, a run is outside its bounds, or a run repeats.</exception>
    private static IReadOnlyList<int> RampSlopes(string path, IReadOnlyList<JsonMember> members)
    {
        string text = ContentValidator.Value(path, members, "rampSlopeRuns", JsonMemberKind.NumberList);
        List<int> runs = [];
        foreach (long item in ContentValidator.Numbers(path, "rampSlopeRuns", text))
        {
            int run = (int)item;
            if (run < Ramp.SteepestRun || run > Ramp.ShallowestRun)
            {
                throw ContentError.Make(path, "rampSlopeRuns", $"holds the run {run}, and a ramp rises one block over {Ramp.SteepestRun} to {Ramp.ShallowestRun} cells (D-346, D-367)");
            }

            foreach (int earlier in runs)
            {
                if (earlier == run)
                {
                    throw ContentError.Make(path, "rampSlopeRuns", $"holds the run {run} twice, and a run that repeats weights its slope twice in the draw (D-390)");
                }
            }

            runs.Add(run);
        }

        return runs;
    }

    /// <summary>One size field, inside its bounds (D-164, D-252).</summary>
    private static int Size(string path, IReadOnlyList<JsonMember> members, string name, int minimum, int maximum)
    {
        long size = Number(path, members, name);
        if (size < minimum || size > maximum)
        {
            throw ContentError.Make(path, name, $"is {size}, and a floor size on this axis is from {minimum} to {maximum} blocks (D-164, D-252)");
        }

        return (int)size;
    }

    /// <summary>One tunnel width: from the cross-section of D-166 to the floor inside its shell, and odd (D-342, D-352).</summary>
    private static int TunnelWidth(string path, IReadOnlyList<JsonMember> members, string name, int sizeX, int sizeZ)
    {
        long width = Number(path, members, name);
        int narrowerSide = sizeX < sizeZ ? sizeX : sizeZ;
        int widest = narrowerSide - ColumnsOutsideDig;
        if (width < SmallestDigSize || width > widest)
        {
            throw ContentError.Make(path, name, $"is {width}, and a tunnel width on a floor of {sizeX} by {sizeZ} blocks is from {SmallestDigSize} to {widest} (D-166, D-352)");
        }

        if (width % 2 == 0)
        {
            throw ContentError.Make(path, name, $"is {width}, and a tunnel width is odd, so the brush of a walker is centered on its cell (D-352)");
        }

        return (int)width;
    }

    /// <summary>One dig height: from the cross-section of D-166 to the rows of the floor past the base row, one floor row, and the top row (D-342, D-352).</summary>
    private static int DigHeight(string path, IReadOnlyList<JsonMember> members, string name, int sizeY)
    {
        long height = Number(path, members, name);
        int tallest = sizeY - RowsOutsideDig;
        if (height < SmallestDigSize || height > tallest)
        {
            throw ContentError.Make(path, name, $"is {height}, and a dig height on a floor of {sizeY} rows is from {SmallestDigSize} to {tallest}, because the base row, one floor row, and the top row stay (D-166, D-352)");
        }

        return (int)height;
    }

    private static long Number(string path, IReadOnlyList<JsonMember> members, string name)
    {
        string text = ContentValidator.Value(path, members, name, JsonMemberKind.Number);
        if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
        {
            throw ContentError.Make(path, name, "holds a number that does not fit a whole number");
        }

        return value;
    }
}
