using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Tools.TextureGen;

namespace WhatYouCarry.Tools.TextureTrace;

/// <summary>A rectangle of a screenshot, in pixels from its top left corner.</summary>
public readonly record struct ImageArea(int X, int Y, int Width, int Height);

/// <summary>One face that the trace reads: the box face, the recipe it writes, and the area of a screenshot that shows the face.</summary>
/// <param name="Model">The content path of the model.</param>
/// <param name="Box">The name of the box in the model.</param>
/// <param name="Side">The face of the box.</param>
/// <param name="Recipe">The name of the recipe file that the trace writes.</param>
/// <param name="Image">The key of the screenshot in the image list of the spec.</param>
/// <param name="At">The area of the screenshot that shows the face.</param>
/// <param name="Turn">The quarter turns clockwise that bring the area upright as the face canvas shows it, from 0 to 3.</param>
/// <param name="Ramps">The index of each palette ramp that a texel of the face can take.</param>
public sealed record TraceFace(string Model, string Box, BoxSide Side, string Recipe, string Image, ImageArea At, int Turn, IReadOnlyList<int> Ramps);

/// <summary>A trace spec: the screenshots, and the faces that the trace reads from them.</summary>
/// <param name="Images">The path of each screenshot relative to the checkout root, by its key.</param>
/// <param name="Faces">The faces, in file order.</param>
public sealed record TraceSpec(IReadOnlyDictionary<string, string> Images, IReadOnlyList<TraceFace> Faces);

/// <summary>
/// The trace spec files under <c>textures/traces/</c> (D-612). A spec names each screenshot of a look reference, and
/// for each box face the area of a screenshot that shows it and the ramps its texels can take. The spec stays in the
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
    private const string AtKey = "at";
    private const string TurnKey = "turn";
    private const string RampsKey = "ramps";

    private static readonly string[] RootFields = [ImagesKey, FacesKey];
    private static readonly string[] FaceFields = [ModelKey, BoxKey, FaceKey, RecipeKey, ImageKey, AtKey, TurnKey, RampsKey];

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

        float[] at = JsonShape.Numbers(path, JsonShape.Member(path, item, owner, AtKey), owner, AtKey, 4);
        ImageArea area = new(Whole(path, owner, at[0]), Whole(path, owner, at[1]), Whole(path, owner, at[2]), Whole(path, owner, at[3]));
        if (area.X < 0 || area.Y < 0 || area.Width < 1 || area.Height < 1)
        {
            throw ContentError.Make(path, AtKey, $"on '{owner}' is [{Text(area.X)}, {Text(area.Y)}, {Text(area.Width)}, {Text(area.Height)}], and an area has a corner at 0 or more and a size of 1 or more");
        }

        int turn = JsonShape.WholeNumber(path, item, owner, TurnKey);
        if (turn < 0 || turn > 3)
        {
            throw ContentError.Make(path, TurnKey, $"on '{owner}' is {Text(turn)}, and a turn is 0 to 3 quarter turns clockwise");
        }

        return new TraceFace(JsonShape.Text(path, item, owner, ModelKey), JsonShape.Text(path, item, owner, BoxKey), side, recipe, image, area, turn, ReadRamps(path, item, owner, palette));
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

    private static int Whole(string path, string owner, float value)
    {
        if (value != (int)value)
        {
            throw ContentError.Make(path, AtKey, $"on '{owner}' holds {value.ToString(CultureInfo.InvariantCulture)}, and an area is whole pixels");
        }

        return (int)value;
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
