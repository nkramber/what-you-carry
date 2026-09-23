using System;

namespace WhatYouCarry.Tests;

/// <summary>
/// The seed count of a sweep on a pull request (D-480, D-481). The CI jobs set <see cref="PullRequestVariable"/> to
/// "1" on a pull request event and to "0" on a push to <c>main</c>. An absent variable gives the full count, so a local
/// run keeps the stronger gate. Any other value is an error that names the variable (T-2).
/// </summary>
public static class SweepScope
{
    public const string PullRequestVariable = "WYC_PR_SWEEP";

    /// <summary>
    /// The test category of the sweep job on the hosted legs (D-479): the procgen, replay, camera, and bot sweeps.
    /// It holds about half of the test time at the pull request count, so the two hosted jobs take about equal time.
    /// </summary>
    public const string SweepCategory = "Sweep";

    /// <summary>A pull request runs one seed in this many of the full count (D-480).</summary>
    public const int PullRequestDivisor = 5;

    /// <summary>The count of seeds of a sweep whose full count is <paramref name="full"/>.</summary>
    /// <exception cref="InvalidOperationException">The variable holds a value other than "0" or "1", or the full count does not divide by the divisor.</exception>
    public static int Seeds(int full)
    {
        return Seeds(full, Environment.GetEnvironmentVariable(PullRequestVariable));
    }

    /// <summary>The count of seeds for one value of the variable. Null is an absent variable.</summary>
    public static int Seeds(int full, string? value)
    {
        if (full % PullRequestDivisor != 0)
        {
            throw new InvalidOperationException($"The full count {full} does not divide by {PullRequestDivisor}, so its pull request count is not exact (D-480).");
        }

        return value switch
        {
            null or "0" => full,
            "1" => full / PullRequestDivisor,
            _ => throw new InvalidOperationException($"The variable {PullRequestVariable} holds '{value}', and it takes '1' on a pull request or '0' on main (D-481)."),
        };
    }
}
