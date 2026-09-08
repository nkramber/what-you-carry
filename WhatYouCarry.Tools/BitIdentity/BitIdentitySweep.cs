using WhatYouCarry.Core.Determinism;

namespace WhatYouCarry.Tools.BitIdentity;

/// <summary>
/// A fixed run of the RNG and of DetMath, folded into one state hash (D-69, D-71). Two platforms that give the
/// same hash agree on every bit of both.
/// </summary>
/// <remarks>
/// <para>
/// Every number here is a constant of this file, so the sweep takes no input and needs no content. The hash
/// changes only when this file changes or when Core changes its numbers. The `bit-identity` CI job runs it on
/// Linux x64, Windows x64, and macOS arm64 and compares the three results.
/// </para>
/// <para>
/// A change to the hash is a change to the simulation. A deliberate one updates
/// <c>BitIdentityKnownAnswer</c> in the test project, and G-20 asks the review to confirm it.
/// </para>
/// </remarks>
public static class BitIdentitySweep
{
    /// <summary>The run seed of the sweep. It has no meaning beyond staying the same.</summary>
    public const ulong RunSeed = 0x5EED1234ABCD9876UL;

    /// <summary>The count of numbers taken from each stream.</summary>
    public const int DrawsPerStream = 1024;

    /// <summary>The count of angle samples across [-4 pi, 4 pi].</summary>
    public const int AngleSamples = 2048;

    /// <summary>The width of one side of the Atan2 grid.</summary>
    public const int AtanGridSide = 64;

    /// <summary>
    /// The streams that the sweep reads. The list is explicit, so a new value of <see cref="RngStream"/> leaves
    /// this hash alone until a later change adds it here on purpose.
    /// </summary>
    private static readonly RngStream[] SweptStreams =
    [
        RngStream.Procgen,
        RngStream.Loot,
        RngStream.Enemy,
        RngStream.Projectile,
    ];

    /// <summary>The hash of the whole sweep.</summary>
    public static StateHash Run()
    {
        StateHash hash = StateHash.Start();
        AddStreams(ref hash);
        AddAngles(ref hash);
        AddRootsAndPowers(ref hash);
        return hash;
    }

    /// <summary>Draws from each stream in turn: the raw word, the float, and a bounded integer.</summary>
    private static void AddStreams(ref StateHash hash)
    {
        foreach (RngStream stream in SweptStreams)
        {
            Rng rng = Rng.ForStream(RunSeed, stream);
            hash.Add((int)stream);
            for (int draw = 0; draw < DrawsPerStream; draw++)
            {
                hash.Add(rng.NextUInt());
                hash.Add(rng.NextFloat());

                // The bound changes with the draw, so the rejection path runs for many different bounds.
                hash.Add(rng.NextInt(draw + 1));
            }
        }
    }

    /// <summary>Sweeps Sin and Cos across [-4 pi, 4 pi] at an even step.</summary>
    private static void AddAngles(ref StateHash hash)
    {
        float start = -4.0f * DetMath.Pi;
        float step = (8.0f * DetMath.Pi) / AngleSamples;
        for (int sample = 0; sample <= AngleSamples; sample++)
        {
            float angle = start + (sample * step);
            hash.Add(DetMath.Sin(angle));
            hash.Add(DetMath.Cos(angle));
        }
    }

    /// <summary>Sweeps Atan2 over a square grid, then Sqrt and Pow over a fixed set of values.</summary>
    private static void AddRootsAndPowers(ref StateHash hash)
    {
        float span = 2.0f * DetMath.Pi;
        float gridStep = (2.0f * span) / AtanGridSide;
        for (int row = 0; row <= AtanGridSide; row++)
        {
            for (int column = 0; column <= AtanGridSide; column++)
            {
                float y = -span + (row * gridStep);
                float x = -span + (column * gridStep);

                // The zero vector has no angle, so the sweep steps over the one grid point that holds it.
                if (y == 0.0f && x == 0.0f)
                {
                    continue;
                }

                hash.Add(DetMath.Atan2(y, x));
            }
        }

        for (int sample = 0; sample <= 512; sample++)
        {
            float value = sample * 0.5f;
            hash.Add(DetMath.Sqrt(value));
            hash.Add(DetMath.Floor((value * 0.375f) - 3.5f));
            hash.Add(DetMath.Clamp(value - 64.0f, -10.0f, 10.0f));
            hash.Add(DetMath.Lerp(-2.5f, 7.25f, value * 0.001953125f));
        }

        for (int exponent = -4; exponent <= 8; exponent++)
        {
            hash.Add(DetMath.Pow(1.5f, exponent));
            hash.Add(DetMath.Pow(-0.75f, exponent));
            hash.Add(DetMath.Pow(2.0f, exponent));
        }
    }
}
