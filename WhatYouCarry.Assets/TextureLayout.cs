using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;

namespace WhatYouCarry.Assets;

/// <summary>The place of one canvas in the atlas, in pixels from the top left corner: the canvas alone, inside its gutter.</summary>
public readonly record struct AtlasRect(int X, int Y, int Width, int Height);

/// <summary>The canvas of one block id and the recipe that paints it (D-505).</summary>
public sealed record BlockPlace(int Block, string Recipe, AtlasRect At);

/// <summary>The canvas of one face of one box of one model, and the recipe that paints it (D-505, D-508).</summary>
public sealed record FacePlace(string Model, string Box, BoxSide Side, string Recipe, AtlasRect At);

/// <summary>
/// The texture layout (D-505): the place of each block canvas and each model face canvas in the atlas. The texture
/// generator of Tools writes it to <c>textures/layout.json</c> beside the atlas, and Game reads it at boot. A test
/// holds the committed file equal to the generator output, as it does for the atlas (D-305).
/// </summary>
/// <remarks>
/// The file lists the blocks by id, and the faces by model, by box in the order of the model file, and by side in
/// the order of <see cref="BoxSide"/>. A place names the canvas in whole pixels. The texel size of a face comes from
/// its box (<see cref="BoxFaces.Texels"/>), so a face of 21.6 texels reads that much of a canvas of 22.
/// </remarks>
public sealed class TextureLayout
{
    /// <summary>The field of the atlas side.</summary>
    public const string AtlasKey = "atlas";

    /// <summary>The field of the list of block places.</summary>
    public const string BlocksKey = "blocks";

    /// <summary>The field of the list of face places.</summary>
    public const string FacesKey = "faces";

    /// <summary>The field of the block id of a block place.</summary>
    public const string BlockKey = "block";

    /// <summary>The field of the model path of a face place.</summary>
    public const string ModelKey = "model";

    /// <summary>The field of the box name of a face place.</summary>
    public const string BoxKey = "box";

    /// <summary>The field of the side name of a face place.</summary>
    public const string FaceKey = "face";

    /// <summary>The field of the recipe name of a place.</summary>
    public const string RecipeKey = "recipe";

    /// <summary>The field of the canvas rectangle of a place: x, y, width, and height in pixels.</summary>
    public const string AtKey = "at";

    private const string RootName = "the file";
    private const int RectLength = 4;
    private static readonly string[] RootFields = [AtlasKey, BlocksKey, FacesKey];
    private static readonly string[] BlockFields = [BlockKey, RecipeKey, AtKey];
    private static readonly string[] FaceFields = [ModelKey, BoxKey, FaceKey, RecipeKey, AtKey];

    private readonly Dictionary<int, BlockPlace> blockById = [];
    private readonly Dictionary<string, FacePlace> faceByKey = [];

    /// <summary>A layout of the given places, in the given order.</summary>
    /// <exception cref="ContextException">Two places name one block or one face.</exception>
    public TextureLayout(IReadOnlyList<BlockPlace> blocks, IReadOnlyList<FacePlace> faces)
    {
        this.Blocks = blocks;
        this.Faces = faces;
        foreach (BlockPlace place in blocks)
        {
            if (!this.blockById.TryAdd(place.Block, place))
            {
                throw new ContextException($"The texture layout places the block {Text(place.Block)} twice, and each block has one canvas.");
            }
        }

        foreach (FacePlace place in faces)
        {
            if (!this.faceByKey.TryAdd(FaceName(place.Model, place.Box, place.Side), place))
            {
                throw new ContextException($"The texture layout places the face {FaceName(place.Model, place.Box, place.Side)} twice, and each face has one canvas.");
            }
        }
    }

    /// <summary>Every block place, in order of the block id.</summary>
    public IReadOnlyList<BlockPlace> Blocks { get; }

    /// <summary>Every face place, by model, by box, and by side.</summary>
    public IReadOnlyList<FacePlace> Faces { get; }

    /// <summary>The name of one face in an error and in the salt of its noise: the model path, the box, and the side.</summary>
    public static string FaceName(string model, string box, BoxSide side)
    {
        return model + ":" + box + ":" + BoxFaces.Name(side);
    }

    /// <summary>The canvas of one block id.</summary>
    /// <exception cref="ContextException">The layout has no canvas for the block.</exception>
    public AtlasRect Block(int block)
    {
        if (!this.blockById.TryGetValue(block, out BlockPlace? place))
        {
            ContextException error = new($"The texture layout has no canvas for the block {Text(block)}. Bind a recipe to the block in {AssetPaths.BlockPaintFile}, and run texture-gen (D-505).");
            error.AddContext("file", AssetPaths.LayoutFile);
            error.AddContext("block", Text(block));
            throw error;
        }

        return place.At;
    }

    /// <summary>The canvas of one face of one box of one model.</summary>
    /// <exception cref="ContextException">The layout has no canvas for the face.</exception>
    public AtlasRect Face(string model, string box, BoxSide side)
    {
        string name = FaceName(model, box, side);
        if (!this.faceByKey.TryGetValue(name, out FacePlace? place))
        {
            ContextException error = new($"The texture layout has no canvas for the face {name}. Name a recipe for the box in the paint file of the model, and run texture-gen (D-508).");
            error.AddContext("file", AssetPaths.LayoutFile);
            error.AddContext("face", name);
            throw error;
        }

        return place.At;
    }

    /// <summary>The layout of one file.</summary>
    /// <exception cref="ContextException">The file is not valid JSON, a field is absent, unknown, or of another kind, the atlas side is not the side of <see cref="AtlasLayout"/>, a side name is not a face, a canvas leaves the atlas, or two places name one block or one face.</exception>
    public static TextureLayout Parse(string path, byte[] bytes)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(bytes);
        }
        catch (JsonException error)
        {
            throw ContentError.MakeForFile(path, $"the file is not valid JSON. {error.Message}");
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw ContentError.MakeForFile(path, "the file must hold one JSON object");
            }

            JsonShape.CheckNoUnknownMember(path, root, RootName, RootFields);
            int atlas = JsonShape.WholeNumber(path, root, RootName, AtlasKey);
            if (atlas != AtlasLayout.AtlasPixels)
            {
                throw ContentError.Make(path, AtlasKey, $"is {Text(atlas)}, and the atlas is {Text(AtlasLayout.AtlasPixels)} pixels on a side (D-506)");
            }

            List<BlockPlace> blocks = [];
            foreach (JsonElement item in List(path, root, BlocksKey))
            {
                string owner = $"{BlocksKey}[{Text(blocks.Count)}]";
                JsonShape.CheckNoUnknownMember(path, item, owner, BlockFields);
                int block = JsonShape.WholeNumber(path, item, owner, BlockKey);
                string recipe = JsonShape.Text(path, item, owner, RecipeKey);
                blocks.Add(new BlockPlace(block, recipe, ReadRect(path, item, owner)));
            }

            List<FacePlace> faces = [];
            foreach (JsonElement item in List(path, root, FacesKey))
            {
                string owner = $"{FacesKey}[{Text(faces.Count)}]";
                JsonShape.CheckNoUnknownMember(path, item, owner, FaceFields);
                string model = JsonShape.Text(path, item, owner, ModelKey);
                string box = JsonShape.Text(path, item, owner, BoxKey);
                string face = JsonShape.Text(path, item, owner, FaceKey);
                if (!BoxFaces.TryParse(face, out BoxSide side))
                {
                    throw ContentError.Make(path, FaceKey, $"on '{owner}' is '{face}', and a face is one of {string.Join(", ", BoxFaces.Names)}");
                }

                string recipe = JsonShape.Text(path, item, owner, RecipeKey);
                faces.Add(new FacePlace(model, box, side, recipe, ReadRect(path, item, owner)));
            }

            try
            {
                return new TextureLayout(blocks, faces);
            }
            catch (ContextException error)
            {
                throw ContentError.MakeForFile(path, error.Message.TrimEnd('.'));
            }
        }
    }

    /// <summary>
    /// The text of the layout file: one place per line, with the fields in a fixed order, and a line feed at the end
    /// of each line. The generator writes it, and a test compares it with the committed file byte for byte.
    /// </summary>
    public string ToJson()
    {
        StringBuilder text = new();
        text.Append("{\n");
        text.Append("  \"").Append(AtlasKey).Append("\": ").Append(Text(AtlasLayout.AtlasPixels)).Append(",\n");
        text.Append("  \"").Append(BlocksKey).Append("\": [\n");
        for (int index = 0; index < this.Blocks.Count; index++)
        {
            BlockPlace place = this.Blocks[index];
            text.Append("    {\"").Append(BlockKey).Append("\": ").Append(Text(place.Block))
                .Append(", \"").Append(RecipeKey).Append("\": ").Append(Quoted(place.Recipe))
                .Append(", \"").Append(AtKey).Append("\": ").Append(RectText(place.At)).Append('}')
                .Append(index + 1 < this.Blocks.Count ? ",\n" : "\n");
        }

        text.Append("  ],\n");
        text.Append("  \"").Append(FacesKey).Append("\": [\n");
        for (int index = 0; index < this.Faces.Count; index++)
        {
            FacePlace place = this.Faces[index];
            text.Append("    {\"").Append(ModelKey).Append("\": ").Append(Quoted(place.Model))
                .Append(", \"").Append(BoxKey).Append("\": ").Append(Quoted(place.Box))
                .Append(", \"").Append(FaceKey).Append("\": ").Append(Quoted(BoxFaces.Name(place.Side)))
                .Append(", \"").Append(RecipeKey).Append("\": ").Append(Quoted(place.Recipe))
                .Append(", \"").Append(AtKey).Append("\": ").Append(RectText(place.At)).Append('}')
                .Append(index + 1 < this.Faces.Count ? ",\n" : "\n");
        }

        text.Append("  ]\n");
        text.Append("}\n");
        return text.ToString();
    }

    /// <summary>One list member of the root.</summary>
    private static JsonElement.ArrayEnumerator List(string path, JsonElement root, string name)
    {
        JsonElement list = JsonShape.Member(path, root, RootName, name);
        if (list.ValueKind != JsonValueKind.Array)
        {
            throw ContentError.Make(path, name, "is not a list");
        }

        return list.EnumerateArray();
    }

    /// <summary>The canvas rectangle of one place: four whole numbers, a positive size, and every pixel inside the atlas.</summary>
    private static AtlasRect ReadRect(string path, JsonElement item, string owner)
    {
        JsonElement list = JsonShape.Member(path, item, owner, AtKey);
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() != RectLength)
        {
            throw ContentError.Make(path, AtKey, $"on '{owner}' is not a list of {Text(RectLength)} whole numbers");
        }

        int[] values = new int[RectLength];
        int index = 0;
        foreach (JsonElement value in list.EnumerateArray())
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out values[index]))
            {
                throw ContentError.Make(path, AtKey, $"on '{owner}' holds a value that is not a whole number, at index {Text(index)}");
            }

            index++;
        }

        // Each end compares the size with the room that the start leaves, so no sum of two large values can wrap (PR #92 review P2-1).
        AtlasRect rect = new(values[0], values[1], values[2], values[3]);
        bool inside = rect.X >= 0 && rect.Y >= 0 && rect.Width > 0 && rect.Height > 0
            && rect.Width <= AtlasLayout.AtlasPixels - rect.X && rect.Height <= AtlasLayout.AtlasPixels - rect.Y;
        if (!inside)
        {
            throw ContentError.Make(path, AtKey, $"on '{owner}' is {RectText(rect)}, and a canvas has a positive size inside the atlas of {Text(AtlasLayout.AtlasPixels)} pixels");
        }

        return rect;
    }

    private static string RectText(AtlasRect rect)
    {
        return $"[{Text(rect.X)}, {Text(rect.Y)}, {Text(rect.Width)}, {Text(rect.Height)}]";
    }

    /// <summary>A JSON string of one name, with the escapes of the platform encoder.</summary>
    private static string Quoted(string value)
    {
        return "\"" + JsonEncodedText.Encode(value).ToString() + "\"";
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
