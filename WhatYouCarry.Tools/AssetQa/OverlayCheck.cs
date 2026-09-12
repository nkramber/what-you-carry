using System.Collections.Generic;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Physics;

namespace WhatYouCarry.Tools.AssetQa;

/// <summary>
/// The overlay check (D-135, D-300): each box of an armor overlay carries the name of the body box that it
/// covers, and it encloses that box on every axis at the rest pose.
/// </summary>
/// <remarks>
/// The body is <see cref="AssetPaths.BodyModel"/>. An overlay box whose name is not a body box name is a
/// finding, because the clip check has no bone to pose it with. A body that did not load gives one finding
/// per overlay, so a broken body does not hide the overlays.
/// </remarks>
public static class OverlayCheck
{
    /// <summary>Every overlay finding of the set, in the order of the overlays, then their boxes.</summary>
    public static IReadOnlyList<AssetFinding> Run(AssetSet set)
    {
        List<AssetFinding> findings = [];
        if (set.Overlays.Count == 0)
        {
            return findings;
        }

        LoadedModel? body = null;
        foreach (LoadedModel candidate in set.Bodies)
        {
            if (candidate.Path == AssetPaths.BodyModel)
            {
                body = candidate;
            }
        }

        foreach (LoadedModel overlay in set.Overlays)
        {
            if (body is null)
            {
                findings.Add(new AssetFinding(overlay.Path, $"is an overlay of '{AssetPaths.BodyModel}', and that body did not load (D-300)"));
                continue;
            }

            foreach (ModelBox box in overlay.Model.Boxes)
            {
                CheckBox(overlay.Path, box, body.Model, findings);
            }
        }

        return findings;
    }

    /// <summary>One overlay box against the body box of its name.</summary>
    private static void CheckBox(string overlayPath, ModelBox box, BlockbenchModel body, List<AssetFinding> findings)
    {
        ModelBox? covered = body.Box(box.Name);
        if (covered is null)
        {
            findings.Add(new AssetFinding(overlayPath, $"the overlay box '{box.Name}' names no box of '{AssetPaths.BodyModel}', and an overlay box carries the name of the body box that it covers (D-300)"));
            return;
        }

        string? axis = FirstOpenAxis(box, covered);
        if (axis is not null)
        {
            findings.Add(new AssetFinding(overlayPath, $"the overlay box '{box.Name}' does not enclose the body box of that name on the {axis} axis (D-135, D-300)"));
        }
    }

    /// <summary>The name of the first axis on which the overlay does not reach past the body box, or null when it encloses it.</summary>
    private static string? FirstOpenAxis(ModelBox overlay, ModelBox covered)
    {
        Vector3 low = overlay.From;
        Vector3 high = overlay.To;
        if (low.X > covered.From.X || high.X < covered.To.X)
        {
            return "X";
        }

        if (low.Y > covered.From.Y || high.Y < covered.To.Y)
        {
            return "Y";
        }

        if (low.Z > covered.From.Z || high.Z < covered.To.Z)
        {
            return "Z";
        }

        return null;
    }
}
