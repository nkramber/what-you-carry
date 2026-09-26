using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Tools.TextureGen;

namespace WhatYouCarry.Tools.TextureTrace;

/// <summary>A point of a screenshot, in pixels from its top left corner. A pixel covers one unit, so the middle of the first pixel is (0.5, 0.5).</summary>
public readonly record struct ImagePoint(double X, double Y);

/// <summary>One face that the trace reads: the box face, the recipe it writes, and the four corners of the face on a screenshot.</summary>
/// <param name="Model">The content path of the model.</param>
/// <param name="Box">The name of the box in the model.</param>
/// <param name="Side">The face of the box.</param>
/// <param name="Recipe">The name of the recipe file that the trace writes.</param>
/// <param name="Image">The key of the screenshot in the image list of the spec.</param>
/// <param name="Corners">
/// The corners of the face on the screenshot, in the order top left, top right, bottom right, and bottom left of the
/// canvas. A part of the reference that leans, such as an arm, or a view that the canvas shows turned, takes its
/// corners in that order, and the trace follows them.
/// </param>
/// <param name="Ramps">The index of each palette ramp that a texel of the face can take.</param>
public sealed record TraceFace(string Model, string Box, BoxSide Side, string Recipe, string Image, IReadOnlyList<ImagePoint> Corners, IReadOnlyList<int> Ramps);

/// <summary>A trace spec: the screenshots, and the faces that the trace reads from them.</summary>
/// <param name="Images">The path of each screenshot relative to the checkout root, by its key.</param>
/// <param name="Faces">The faces, in file order.</param>
public sealed record TraceSpec(IReadOnlyDictionary<string, string> Images, IReadOnlyList<TraceFace> Faces);

/// <summary>
/// The trace spec files under <c>textures/traces/</c> (D-612). A spec names each screenshot of a look reference, and
/// for each box face the four corners of the face on a screenshot and the ramps its texels can take. The spec stays in the
/// repository as the record of each trace, and the screenshots stay in the reference folder (D-496, D-614).
/// </summary>
public static class TraceSpecFile
{
    private const string ImagesKey = "images";
    private const string FacesKey = "faces";
    private const string ModelKey = "model";
    private const string BoxKey = "box";
    private const string FaceKey = "face";
    private const string RecipeKey = "recipe";
    private const string ImageKey = "image";
    private const string CornersKey = "corners";
    private const string RampsKey = "ramps";

    private static readonly string[] RootFields = [ImagesKey, FacesKey];
    private static readonly string[] FaceFields = [ModelKey, BoxKey, FaceKey, RecipeKey, ImageKey, CornersKey, RampsKey];

    /// <summary>The spec of one file.</summary>
    /// <param name="path">The content path of the file, for an error.</param>
    /// <param name="bytes">The bytes of the file.</param>
    /// <param name="palette">The palette that each ramp name must name.</param>
    /// <exception cref="WhatYouCarry.Core.Logging.ContextException">The file is not a valid spec.</exception>
    public static TraceSpec Parse(string path, byte[] bytes, Palette palette)
    {
        using JsonDocument document = TextureJson.Parse(path, bytes);
        JsonElement root = document.RootElement;
        JsonShape.CheckNoUnknownMember(path, root, TextureJson.RootName, RootFields);

        JsonElement imageList = JsonShape.Member(path, root, TextureJson.RootName, ImagesKey);
        if (imageList.ValueKind != JsonValueKind.Object || !imageList.EnumerateObject().MoveNext())
        {
            throw ContentError.Make(path, ImagesKey, "is not an object that maps at least one key to the path of a screenshot");
        }

        Dictionary<string, string> images = [];
        foreach (JsonProperty image in imageList.EnumerateObject())
        {
            images.Add(image.Name, JsonShape.Text(path, imageList, ImagesKey, image.Name));
        }

        JsonElement faceList = JsonShape.Member(path, root, TextureJson.RootName, FacesKey);
        if (faceList.ValueKind != JsonValueKind.Array || faceList.GetArrayLength() == 0)
        {
            throw ContentError.Make(path, FacesKey, "is not a list that holds at least one face");
        }

        List<TraceFace> faces = [];
        HashSet<string> recipes = [];
        HashSet<string> boxFaces = [];
        foreach (JsonElement item in faceList.EnumerateArray())
        {
            string owner = $"{FacesKey}[{Text(faces.Count)}]";
            TraceFace face = ReadFace(path, item, owner, images, palette);
            if (!recipes.Add(face.Recipe))
            {
                throw ContentError.Make(path, RecipeKey, $"on '{owner}' names the recipe '{face.Recipe}' a second time, and each face writes a recipe of its own");
            }

            if (!boxFaces.Add(TextureLayout.FaceName(face.Model, face.Box, face.Side)))
            {
                throw ContentError.Make(path, FaceKey, $"on '{owner}' names the face {TextureLayout.FaceName(face.Model, face.Box, face.Side)} a second time");
            }

            faces.Add(face);
        }

        return new TraceSpec(images, faces);
    }

    private static TraceFace ReadFace(string path, JsonElement item, string owner, Dictionary<string, string> images, Palette palette)
    {
        JsonShape.CheckNoUnknownMember(path, item, owner, FaceFields);
        string faceName = JsonShape.Text(path, item, owner, FaceKey);
        if (!BoxFaces.TryParse(faceName, out BoxSide side))
        {
            throw ContentError.Make(path, FaceKey, $"on '{owner}' is '{faceName}', and a face is one of {string.Join(", ", BoxFaces.Names)}");
        }

        string recipe = JsonShape.Text(path, item, owner, RecipeKey);
        if (!IsRecipeName(recipe))
        {
            throw ContentError.Make(path, RecipeKey, $"on '{owner}' is '{recipe}', and a recipe name of a trace holds lowercase letters, digits, and hyphens alone, so it is a safe file name");
        }

        string image = JsonShape.Text(path, item, owner, ImageKey);
        if (!images.ContainsKey(image))
        {
            throw ContentError.Make(path, ImageKey, $"on '{owner}' is '{image}', and the image list has no key of that name");
        }

        return new TraceFace(JsonShape.Text(path, item, owner, ModelKey), JsonShape.Text(path, item, owner, BoxKey), side, recipe, image, ReadCorners(path, item, owner), ReadRamps(path, item, owner, palette));
    }

    /// <summary>
    /// The four corners of a face. Each corner is 0 or more on each axis. The corners run clockwise on the screen, and
    /// the face bends out at each one, because a crossed or folded order traces the face out of shape.
    /// </summary>
    private static List<ImagePoint> ReadCorners(string path, JsonElement item, string owner)
    {
        JsonElement list = JsonShape.Member(path, item, owner, CornersKey);
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() != 4)
        {
            throw ContentError.Make(path, CornersKey, $"on '{owner}' is not a list of four corners: the top left, the top right, the bottom right, and the bottom left");
        }

        List<ImagePoint> corners = [];
        foreach (JsonElement corner in list.EnumerateArray())
        {
            float[] point = JsonShape.Numbers(path, corner, owner, CornersKey, 2);
            if (point[0] < 0.0f || point[1] < 0.0f)
            {
                throw ContentError.Make(path, CornersKey, $"on '{owner}' holds a corner at ({Number(point[0])}, {Number(point[1])}), and a corner is 0 or more on each axis");
            }

            corners.Add(new ImagePoint(point[0], point[1]));
        }

        for (int index = 0; index < 4; index++)
        {
            ImagePoint before = corners[(index + 3) % 4];
            ImagePoint at = corners[index];
            ImagePoint after = corners[(index + 1) % 4];
            // With y down the screen, a clockwise turn at a corner gives a positive cross product.
            double turn = ((at.X - before.X) * (after.Y - at.Y)) - ((at.Y - before.Y) * (after.X - at.X));
            if (turn <= 0.0)
            {
                throw ContentError.Make(path, CornersKey, $"on '{owner}' does not run clockwise at the corner {Text(index)}: give the top left, the top right, the bottom right, and the bottom left of the face, with no crossed side");
            }
        }

        return corners;
    }

    private static List<int> ReadRamps(string path, JsonElement item, string owner, Palette palette)
    {
        JsonElement list = JsonShape.Member(path, item, owner, RampsKey);
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() == 0)
        {
            throw ContentError.Make(path, RampsKey, $"on '{owner}' is not a list that names at least one ramp");
        }

        List<int> ramps = [];
        foreach (JsonElement name in list.EnumerateArray())
        {
            string text = name.ValueKind == JsonValueKind.String ? name.GetString() ?? string.Empty : string.Empty;
            int ramp = RampIndex(palette, text);
            if (ramp < 0)
            {
                throw ContentError.Make(path, RampsKey, $"on '{owner}' names the ramp '{text}', and the palette has no ramp of that name");
            }

            if (ramps.Contains(ramp))
            {
                throw ContentError.Make(path, RampsKey, $"on '{owner}' names the ramp '{text}' twice");
            }

            ramps.Add(ramp);
        }

        return ramps;
    }

    private static int RampIndex(Palette palette, string name)
    {
        for (int ramp = 0; ramp < palette.Ramps.Count; ramp++)
        {
            if (palette.Ramps[ramp].Name == name)
            {
                return ramp;
            }
        }

        return -1;
    }

    private static bool IsRecipeName(string name)
    {
        if (name.Length == 0)
        {
            return false;
        }

        foreach (char letter in name)
        {
            if (!(letter is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-'))
            {
                return false;
            }
        }

        return true;
    }

    private static string Number(float value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
