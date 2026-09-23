using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using WhatYouCarry.Assets;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.World;

namespace WhatYouCarry.Tools.TextureGen;

/// <summary>The recipe of one block id (D-505).</summary>
public sealed record BlockPaint(int Block, string Recipe);

/// <summary>The recipe of each face of each box of one model, in the order of the boxes and of <see cref="BoxSide"/> (D-508).</summary>
/// <param name="Model">The model that the paint file names.</param>
/// <param name="Recipes">For each box of the model, in its order, the recipe name of each side.</param>
public sealed record ModelPaint(BlockbenchModel Model, IReadOnlyList<IReadOnlyList<string>> Recipes);

/// <summary>
/// The two kinds of binding file (D-505, D-508). <c>textures/blocks.json</c> binds a recipe to each block id. A paint
/// file next to a model, <c>models/&lt;model&gt;.paint.json</c>, binds a recipe to each box of the model, and to a single
/// face where that face differs. Both are files of this project, so the unknown-field check of D-168 applies.
/// </summary>
public static class PaintFile
{
    /// <summary>The field of the block list of the block file.</summary>
    public const string BlocksKey = "blocks";

    /// <summary>The field of the block id of one entry of the block file.</summary>
    public const string BlockKey = "block";

    /// <summary>The field of a recipe name.</summary>
    public const string RecipeKey = "recipe";

    /// <summary>The field of the model path of a paint file.</summary>
    public const string ModelKey = "model";

    /// <summary>The field of the box object of a paint file.</summary>
    public const string BoxesKey = "boxes";

    /// <summary>The field of the face overrides of one box of a paint file.</summary>
    public const string FacesKey = "faces";

    private static readonly string[] BlockFileFields = [BlocksKey];
    private static readonly string[] BlockFields = [BlockKey, RecipeKey];
    private static readonly string[] PaintFields = [ModelKey, BoxesKey];
    private static readonly string[] BoxFields = [RecipeKey, FacesKey];

    /// <summary>
    /// The block file: one recipe for each block id of <see cref="BlockId"/> except air, in order of the id. A ramp cell
    /// takes the canvas of raw stone (D-368), so it has no entry.
    /// </summary>
    /// <exception cref="ContextException">The file is not valid, an id is not a block or is air, an id appears twice or not at all, or a recipe does not exist.</exception>
    public static IReadOnlyList<BlockPaint> ReadBlocks(string path, byte[] bytes, IReadOnlyDictionary<string, Recipe> recipes)
    {
        using JsonDocument document = TextureJson.Parse(path, bytes);
        JsonElement root = document.RootElement;
        JsonShape.CheckNoUnknownMember(path, root, TextureJson.RootName, BlockFileFields);
        JsonElement list = JsonShape.Member(path, root, TextureJson.RootName, BlocksKey);
        if (list.ValueKind != JsonValueKind.Array)
        {
            throw ContentError.Make(path, BlocksKey, "is not a list");
        }

        SortedDictionary<int, BlockPaint> byId = [];
        int index = 0;
        foreach (JsonElement item in list.EnumerateArray())
        {
            string owner = $"{BlocksKey}[{Text(index)}]";
            JsonShape.CheckNoUnknownMember(path, item, owner, BlockFields);
            int block = JsonShape.WholeNumber(path, item, owner, BlockKey);
            bool isBlock = block > (int)BlockId.Air && block <= byte.MaxValue && Enum.IsDefined((BlockId)block);
            if (!isBlock)
            {
                throw ContentError.Make(path, BlockKey, $"on '{owner}' is {Text(block)}, and a block entry names a block id of D-259 other than air");
            }

            string recipe = ReadRecipe(path, item, owner, recipes);
            if (!byId.TryAdd(block, new BlockPaint(block, recipe)))
            {
                throw ContentError.Make(path, BlockKey, $"on '{owner}' is {Text(block)}, and an earlier entry names that block");
            }

            index++;
        }

        foreach (BlockId block in Enum.GetValues<BlockId>())
        {
            if (block != BlockId.Air && !byId.ContainsKey((int)block))
            {
                throw ContentError.Make(path, BlocksKey, $"has no entry for the block {Text((int)block)} ({block}), and every block id other than air has a recipe");
            }
        }

        return [.. byId.Values];
    }

    /// <summary>One paint file: the recipe of each face of each box of its model.</summary>
    /// <param name="path">The content path of the paint file.</param>
    /// <param name="bytes">The bytes of the paint file.</param>
    /// <param name="model">The model that the file names. Its path is <see cref="AssetPaths.PaintPath"/> of the paint file.</param>
    /// <param name="recipes">Every recipe, by name.</param>
    /// <exception cref="ContextException">The file is not valid, it names another model, a box of the model has no entry, an entry names no box of the model, a face name is not a face, or a recipe does not exist.</exception>
    public static ModelPaint ReadModel(string path, byte[] bytes, BlockbenchModel model, IReadOnlyDictionary<string, Recipe> recipes)
    {
        using JsonDocument document = TextureJson.Parse(path, bytes);
        JsonElement root = document.RootElement;
        JsonShape.CheckNoUnknownMember(path, root, TextureJson.RootName, PaintFields);
        string modelPath = JsonShape.Text(path, root, TextureJson.RootName, ModelKey);
        if (modelPath != model.Path || AssetPaths.PaintPath(modelPath) != path)
        {
            throw ContentError.Make(path, ModelKey, $"is '{modelPath}', and the paint file {path} names the model {model.Path} (D-508)");
        }

        JsonElement boxes = JsonShape.Member(path, root, TextureJson.RootName, BoxesKey);
        if (boxes.ValueKind != JsonValueKind.Object)
        {
            throw ContentError.Make(path, BoxesKey, "is not an object of box names");
        }

        HashSet<string> named = [];
        foreach (JsonProperty entry in boxes.EnumerateObject())
        {
            if (model.Box(entry.Name) is null)
            {
                throw ContentError.Make(path, BoxesKey, $"names the box '{entry.Name}', and the model {model.Path} has no box of that name");
            }

            if (!named.Add(entry.Name))
            {
                throw ContentError.Make(path, BoxesKey, $"names the box '{entry.Name}' twice, and each box has one entry");
            }
        }

        List<IReadOnlyList<string>> recipesOfBoxes = [];
        foreach (ModelBox box in model.Boxes)
        {
            if (!boxes.TryGetProperty(box.Name, out JsonElement entry))
            {
                throw ContentError.Make(path, BoxesKey, $"has no entry for the box '{box.Name}' of {model.Path}, and every box names its recipe (D-508)");
            }

            recipesOfBoxes.Add(ReadBox(path, entry, box.Name, recipes));
        }

        return new ModelPaint(model, recipesOfBoxes);
    }

    /// <summary>The recipe of each side of one box: the recipe of the box, with each face override in its place.</summary>
    private static string[] ReadBox(string path, JsonElement entry, string boxName, IReadOnlyDictionary<string, Recipe> recipes)
    {
        string owner = $"{BoxesKey}.{boxName}";
        JsonShape.CheckNoUnknownMember(path, entry, owner, BoxFields);
        string boxRecipe = ReadRecipe(path, entry, owner, recipes);
        string[] sides = [boxRecipe, boxRecipe, boxRecipe, boxRecipe, boxRecipe, boxRecipe];
        JsonElement faces = JsonShape.Member(path, entry, owner, FacesKey);
        if (faces.ValueKind != JsonValueKind.Object)
        {
            throw ContentError.Make(path, FacesKey, $"on '{owner}' is not an object of face names");
        }

        HashSet<string> seen = [];
        foreach (JsonProperty face in faces.EnumerateObject())
        {
            if (!BoxFaces.TryParse(face.Name, out BoxSide side) || !seen.Add(face.Name))
            {
                throw ContentError.Make(path, FacesKey, $"on '{owner}' names '{face.Name}', and each override names one of {string.Join(", ", BoxFaces.Names)} once");
            }

            string recipe = face.Value.ValueKind == JsonValueKind.String ? face.Value.GetString() ?? string.Empty : string.Empty;
            CheckRecipe(path, FacesKey, $"{owner}.{face.Name}", recipe, recipes);
            sides[(int)side] = recipe;
        }

        return sides;
    }

    private static string ReadRecipe(string path, JsonElement item, string owner, IReadOnlyDictionary<string, Recipe> recipes)
    {
        string recipe = JsonShape.Text(path, item, owner, RecipeKey);
        CheckRecipe(path, RecipeKey, owner, recipe, recipes);
        return recipe;
    }

    private static void CheckRecipe(string path, string field, string owner, string recipe, IReadOnlyDictionary<string, Recipe> recipes)
    {
        if (!recipes.ContainsKey(recipe))
        {
            throw ContentError.Make(path, field, $"on '{owner}' names the recipe '{recipe}', and no recipe file under {AssetPaths.RecipeDirectory} has that name");
        }
    }

    private static string Text(int number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
    }
}
