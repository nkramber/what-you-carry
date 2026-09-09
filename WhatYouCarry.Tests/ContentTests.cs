using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Logging;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The content loader, the validators, the hash, and the string table (PR-5 exit tests 1 to 5).</summary>
public sealed class ContentTests
{
    /// <summary>A source that holds its files in memory, which is the test half of the two callers of D-111.</summary>
    private sealed class MemorySource : IContentSource
    {
        private readonly List<ContentFile> files = [];

        public MemorySource Add(string path, string text)
        {
            this.files.Add(new ContentFile(path, Encoding.UTF8.GetBytes(text)));
            return this;
        }

        public IReadOnlyList<ContentFile> Read() => this.files;
    }

    private const string Floor = """
        {"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine","sizeX":48,"sizeY":12,"sizeZ":48}
        """;

    private const string ChamberKindText = """
        {"id":"k","weight":10,"boxCountMin":1,"boxCountMax":3,"boxSizeMin":3,"boxSizeMax":6}
        """;

    private const string StringTable = """
        {"hub.descend":"Descend"}
        """;

    private static MemorySource Valid() => new MemorySource()
        .Add("floors/a.json", Floor)
        .Add(Strings.FilePath, StringTable);

    /// <summary>PR-5 exit test 1. An absent field names the file, the field, and the reason (D-92, T-2).</summary>
    [Fact]
    public void AbsentFieldNamesField()
    {
        MemorySource source = new MemorySource()
            .Add("floors/a.json", """{"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"band":"working-mine","sizeX":48,"sizeY":12,"sizeZ":48}""")
            .Add(Strings.FilePath, StringTable);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("floors/a.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("difficultyBudget", error.Message, StringComparison.Ordinal);
        Assert.Contains("is absent", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Every required field of a floor template is required, one at a time.</summary>
    [Theory]
    [InlineData("id")]
    [InlineData("minDepth")]
    [InlineData("maxDepth")]
    [InlineData("roomCountMin")]
    [InlineData("roomCountMax")]
    [InlineData("difficultyBudget")]
    [InlineData("band")]
    [InlineData("sizeX")]
    [InlineData("sizeY")]
    [InlineData("sizeZ")]
    public void EveryRequiredFloorFieldIsRequired(string omitted)
    {
        List<JsonMember> members = [];
        foreach (JsonMember member in JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(Floor)))
        {
            if (member.Name != omitted)
            {
                members.Add(member);
            }
        }

        ContextException error = Assert.Throws<ContextException>(() => FloorTemplate.FromMembers("floors/a.json", members));
        Assert.Contains(omitted, error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-5 exit test 2. An unknown field is a failure, and never a field that the loader steps over.</summary>
    [Fact]
    public void UnknownFieldFails()
    {
        MemorySource source = new MemorySource()
            .Add("floors/a.json", """{"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine","sizeX":48,"sizeY":12,"sizeZ":48,"extra":1}""")
            .Add(Strings.FilePath, StringTable);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("extra", error.Message, StringComparison.Ordinal);
        Assert.Contains("is not a field", error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-5 exit test 3. Every file under `content/` loads, and the set holds what the game needs.</summary>
    [Fact]
    public void EveryContentFileLoads()
    {
        ContentSet set = new ContentLoader(new RepositoryContentSource()).Load();

        Assert.Equal(3, set.Floors.Count);
        Assert.Equal(8, set.Chambers.Count);
        Assert.Equal(6, set.Projectiles.Count);
        Assert.True(set.Strings.Count > 0);
        Assert.Equal(64, set.Hash.Length);

        // The three floor bands of D-210 cover floors 1 to 15 with no gap and no overlap.
        long[] covered = new long[16];
        foreach (FloorTemplate floor in set.Floors)
        {
            for (long depth = floor.MinDepth; depth <= floor.MaxDepth; depth++)
            {
                covered[depth]++;
            }
        }

        for (int depth = 1; depth <= 15; depth++)
        {
            Assert.Equal(1, covered[depth]);
        }
    }

    /// <summary>PR-5 exit test 4. An unknown string id throws, because a blank line on a screen hides it (T-2).</summary>
    [Fact]
    public void UnknownStringIdThrows()
    {
        Strings strings = new ContentLoader(new RepositoryContentSource()).Load().Strings;

        ContextException error = Assert.Throws<ContextException>(() => strings.Get("no.such.id"));
        Assert.Contains("no.such.id", error.Message, StringComparison.Ordinal);
        Assert.False(strings.Has("no.such.id"));

        Assert.Equal("Descend", strings.Get("hub.descend"));
        Assert.True(strings.Has("hub.descend"));
    }

    /// <summary>PR-5 exit test 5. Two loads of one set give one hash, and one byte of change gives another.</summary>
    [Fact]
    public void ContentHashIsStable()
    {
        string first = new ContentLoader(Valid()).Load().Hash;
        string second = new ContentLoader(Valid()).Load().Hash;
        Assert.Equal(first, second);

        MemorySource changed = new MemorySource()
            .Add("floors/a.json", Floor.Replace("100", "101", StringComparison.Ordinal))
            .Add(Strings.FilePath, StringTable);
        Assert.NotEqual(first, new ContentLoader(changed).Load().Hash);

        // The order of the source does not change the hash, and the path does.
        MemorySource reordered = new MemorySource()
            .Add(Strings.FilePath, StringTable)
            .Add("floors/a.json", Floor);
        Assert.Equal(first, new ContentLoader(reordered).Load().Hash);

        MemorySource renamed = new MemorySource()
            .Add("floors/b.json", Floor)
            .Add(Strings.FilePath, StringTable);
        Assert.NotEqual(first, new ContentLoader(renamed).Load().Hash);
    }

    /// <summary>
    /// Each file enters the hash with its lengths, so no two content sets share one input. Without them the
    /// path `a` with the bytes `bc` and the path `ab` with the byte `c` both give `abc` (F-78).
    /// </summary>
    [Fact]
    public void TheHashFramesEachFile()
    {
        // The review trigger. Both sets gave one hash before the lengths.
        string first = ContentHash.Of([new ContentFile("a", Encoding.UTF8.GetBytes("bc"))]);
        string second = ContentHash.Of([new ContentFile("ab", Encoding.UTF8.GetBytes("c"))]);
        Assert.NotEqual(first, second);

        // A boundary that moves between two files gives another hash too.
        string third = ContentHash.Of([new ContentFile("a", Encoding.UTF8.GetBytes("b")), new ContentFile("c", Encoding.UTF8.GetBytes("d"))]);
        string fourth = ContentHash.Of([new ContentFile("a", Encoding.UTF8.GetBytes("bc")), new ContentFile("d", [])]);
        Assert.NotEqual(third, fourth);

        // An empty file still counts, and it changes the hash.
        Assert.NotEqual(
            ContentHash.Of([new ContentFile("a", Encoding.UTF8.GetBytes("b"))]),
            ContentHash.Of([new ContentFile("a", Encoding.UTF8.GetBytes("b")), new ContentFile("b", [])]));
    }

    /// <summary>
    /// A number that no whole number holds carries the file, the field, and the reason. The reader keeps the
    /// token text, and the validator owns the shape (D-220, D-92, F-78).
    /// </summary>
    [Theory]
    [InlineData("1.5")]
    [InlineData("99999999999999999999")]
    [InlineData("-99999999999999999999")]
    [InlineData("1e400")]
    public void ANumberThatNoWholeNumberHoldsNamesTheField(string number)
    {
        string text = Floor.Replace("\"minDepth\":1", $"\"minDepth\":{number}", StringComparison.Ordinal);

        // The reader keeps the text, so it raises no error of its own.
        IReadOnlyList<JsonMember> members = JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(text));
        Assert.Equal(number, ContentValidator.Value("floors/a.json", members, "minDepth", JsonMemberKind.Number));

        // The validator names the file, the field, and the reason.
        ContextException error = Assert.Throws<ContextException>(() => FloorTemplate.FromMembers("floors/a.json", members));
        Assert.Contains("floors/a.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("minDepth", error.Message, StringComparison.Ordinal);
        Assert.Contains("whole number", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Two files of one path make the order matter, and the hash must not depend on it (T-2).</summary>
    [Fact]
    public void ARepeatedContentPathIsAnError()
    {
        MemorySource source = new MemorySource()
            .Add("floors/a.json", Floor)
            .Add("floors/a.json", Floor)
            .Add(Strings.FilePath, StringTable);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("share this path", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A file that no content type claims is an error, and never a file that the loader steps over.</summary>
    [Fact]
    public void AnUnclaimedPathIsAnError()
    {
        MemorySource source = Valid().Add("sounds/a.json", "{}");
        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("sounds/a.json", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A set with no string table is an error, because every screen needs one (D-98).</summary>
    [Fact]
    public void ASetWithNoStringTableIsAnError()
    {
        MemorySource source = new MemorySource().Add("floors/a.json", Floor);
        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains(Strings.FilePath, error.Message, StringComparison.Ordinal);
    }

    /// <summary>Two records of one type must not share an id, because a lookup would take either one.</summary>
    [Fact]
    public void ARepeatedIdIsAnError()
    {
        MemorySource source = new MemorySource()
            .Add("floors/a.json", Floor)
            .Add("floors/b.json", Floor)
            .Add(Strings.FilePath, StringTable);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("two floor templates", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A file that is not one JSON object is an error that names the file (T-2).</summary>
    [Theory]
    [InlineData("")]
    [InlineData("[]")]
    [InlineData("{")]
    [InlineData("{\"a\":1}{\"b\":2}")]
    [InlineData("{\"a\":1,\"a\":2}")]
    public void AFileThatIsNotOneObjectIsAnError(string text)
    {
        ContextException error = Assert.Throws<ContextException>(() => JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(text)));
        Assert.Contains("floors/a.json", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A field of the wrong kind names the kind it holds and the kind the type needs (T-2).</summary>
    [Fact]
    public void AFieldOfTheWrongKindIsAnError()
    {
        MemorySource source = new MemorySource()
            .Add("floors/a.json", """{"id":1,"minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine","sizeX":48,"sizeY":12,"sizeZ":48}""")
            .Add(Strings.FilePath, StringTable);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("holds a number value", error.Message, StringComparison.Ordinal);
        Assert.Contains("needs a text value", error.Message, StringComparison.Ordinal);
    }

    /// <summary>An empty list and a null are kinds of their own, because the run record header carries both (D-229).</summary>
    [Fact]
    public void AnEmptyListAndANullAreKinds()
    {
        IReadOnlyList<JsonMember> members = JsonObjectReader.Read("header", Encoding.UTF8.GetBytes("{\"loadout\":[],\"amulet\":null}"));

        Assert.Equal(2, members.Count);
        Assert.Equal(new JsonMember("loadout", string.Empty, JsonMemberKind.EmptyList), members[0]);
        Assert.Equal(new JsonMember("amulet", string.Empty, JsonMemberKind.Null), members[1]);
    }

    /// <summary>A list with an item, a nested object, and a list that the file ends inside are each an error that names the field (D-229).</summary>
    [Theory]
    [InlineData("{\"loadout\":[1]}", "with an item")]
    [InlineData("{\"loadout\":[[]]}", "with an item")]
    [InlineData("{\"loadout\":{}}", "no Phase 1 type uses")]
    [InlineData("{\"loadout\":[", "not valid JSON")]
    public void AListWithAnItemIsAnError(string text, string reason)
    {
        ContextException error = Assert.Throws<ContextException>(() => JsonObjectReader.Read("header", Encoding.UTF8.GetBytes(text)));
        Assert.Contains("header", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A floor template with an impossible range is an error, and never a band that covers no floor.</summary>
    [Theory]
    [InlineData("\"minDepth\":6,\"maxDepth\":5", "minDepth")]
    [InlineData("\"roomCountMin\":9,\"roomCountMax\":8", "roomCountMin")]
    [InlineData("\"roomCountMin\":0,\"roomCountMax\":8", "roomCountMin")]
    public void AnImpossibleFloorRangeIsAnError(string replacement, string field)
    {
        string text = replacement.StartsWith("\"minDepth\"", StringComparison.Ordinal)
            ? Floor.Replace("\"minDepth\":1,\"maxDepth\":5", replacement, StringComparison.Ordinal)
            : Floor.Replace("\"roomCountMin\":4,\"roomCountMax\":8", replacement, StringComparison.Ordinal);

        ContextException error = Assert.Throws<ContextException>(
            () => FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
    }

    /// <summary>An optional field can stand absent, and the record then holds its zero.</summary>
    [Fact]
    public void AnOptionalFieldCanStandAbsent()
    {
        ContentSet set = new ContentLoader(new RepositoryContentSource()).Load();
        foreach (ProjectileDefinition projectile in set.Projectiles)
        {
            if (projectile.Id == "musket-ball")
            {
                Assert.Equal(0, projectile.AreaCentimetres);
            }

            if (projectile.Id == "arrow")
            {
                Assert.Equal(150, projectile.AreaCentimetres);
            }
        }
    }

    /// <summary>Every required field of a chamber kind is required, one at a time (D-255).</summary>
    [Theory]
    [InlineData("id")]
    [InlineData("weight")]
    [InlineData("boxCountMin")]
    [InlineData("boxCountMax")]
    [InlineData("boxSizeMin")]
    [InlineData("boxSizeMax")]
    public void EveryRequiredChamberKindFieldIsRequired(string omitted)
    {
        List<JsonMember> members = [];
        foreach (JsonMember member in JsonObjectReader.Read("chambers/k.json", Encoding.UTF8.GetBytes(ChamberKindText)))
        {
            if (member.Name != omitted)
            {
                members.Add(member);
            }
        }

        ContextException error = Assert.Throws<ContextException>(() => ChamberKind.FromMembers("chambers/k.json", members));
        Assert.Contains(omitted, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A chamber kind outside its bounds is an error that names the field: a weight below one, a box side below the tunnel width, a range upside down (D-166, D-255).</summary>
    [Theory]
    [InlineData("\"weight\":10", "\"weight\":0", "weight")]
    [InlineData("\"boxCountMin\":1", "\"boxCountMin\":0", "boxCountMin")]
    [InlineData("\"boxCountMin\":1", "\"boxCountMin\":4", "boxCountMin")]
    [InlineData("\"boxCountMax\":3", "\"boxCountMax\":9", "boxCountMax")]
    [InlineData("\"boxSizeMin\":3", "\"boxSizeMin\":2", "boxSizeMin")]
    [InlineData("\"boxSizeMin\":3", "\"boxSizeMin\":7", "boxSizeMin")]
    [InlineData("\"boxSizeMax\":6", "\"boxSizeMax\":17", "boxSizeMax")]
    public void AChamberKindOutsideItsBoundsIsAnError(string from, string to, string field)
    {
        string text = ChamberKindText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(ChamberKindText, text);

        ContextException error = Assert.Throws<ContextException>(
            () => ChamberKind.FromMembers("chambers/k.json", JsonObjectReader.Read("chambers/k.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A floor size past the grid limit, or below the smallest floor the dig plan can carve, is an error that names the axis (D-164, D-252).</summary>
    [Theory]
    [InlineData("\"sizeX\":48", "\"sizeX\":23", "sizeX")]
    [InlineData("\"sizeX\":48", "\"sizeX\":129", "sizeX")]
    [InlineData("\"sizeY\":12", "\"sizeY\":6", "sizeY")]
    [InlineData("\"sizeY\":12", "\"sizeY\":33", "sizeY")]
    [InlineData("\"sizeZ\":48", "\"sizeZ\":0", "sizeZ")]
    [InlineData("\"difficultyBudget\":100", "\"difficultyBudget\":9", "difficultyBudget")]
    public void AFloorSizeOutsideItsBoundsIsAnError(string from, string to, string field)
    {
        string text = Floor.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(Floor, text);

        ContextException error = Assert.Throws<ContextException>(
            () => FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Two chamber kinds with one id are an error, because the draw would take either one.</summary>
    [Fact]
    public void ARepeatedChamberIdIsAnError()
    {
        MemorySource source = Valid()
            .Add("chambers/k.json", ChamberKindText)
            .Add("chambers/other.json", ChamberKindText);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("two chamber kinds", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The chamber kinds of the checkout load with their weights, and the floors carry their sizes by band (D-252, D-255).</summary>
    [Fact]
    public void TheChamberKindsAndFloorSizesLoad()
    {
        ContentSet set = TestWorld.Content;

        long lightest = long.MaxValue;
        long heaviest = 0;
        foreach (ChamberKind kind in set.Chambers)
        {
            lightest = Math.Min(lightest, kind.Weight);
            heaviest = Math.Max(heaviest, kind.Weight);
            Assert.True(kind.BoxSizeMin >= ChamberKind.SmallestBoxSide, $"The kind '{kind.Id}' has a box side of {kind.BoxSizeMin}, below the tunnel width.");
        }

        Assert.Equal(8, lightest);
        Assert.Equal(40, heaviest);

        foreach (FloorTemplate floor in set.Floors)
        {
            int expected = floor.MinDepth == 1 ? 48 : floor.MinDepth == 6 ? 72 : 96;
            int expectedHeight = floor.MinDepth == 1 ? 12 : floor.MinDepth == 6 ? 16 : 20;
            Assert.Equal(expected, floor.SizeX);
            Assert.Equal(expectedHeight, floor.SizeY);
            Assert.Equal(expected, floor.SizeZ);
        }
    }

    /// <summary>
    /// The order of the source does not reach the typed lists: two sources with the same files in two orders
    /// give the chamber kinds and the floors in one order, the ordinal order of the paths, so the budget draw
    /// digs one floor from one seed on every file system (D-159, G-9; PR #27 automated pass).
    /// </summary>
    [Fact]
    public void TheSourceOrderDoesNotReachTheLists()
    {
        MemorySource forward = new MemorySource()
            .Add("chambers/a.json", ChamberKindText.Replace("\"k\"", "\"a\"", StringComparison.Ordinal))
            .Add("chambers/b.json", ChamberKindText.Replace("\"k\"", "\"b\"", StringComparison.Ordinal))
            .Add("floors/a.json", Floor)
            .Add("floors/b.json", Floor.Replace("\"id\":\"a\"", "\"id\":\"b\"", StringComparison.Ordinal).Replace("\"minDepth\":1,\"maxDepth\":5", "\"minDepth\":6,\"maxDepth\":9", StringComparison.Ordinal))
            .Add(Strings.FilePath, StringTable);
        MemorySource backward = new MemorySource()
            .Add(Strings.FilePath, StringTable)
            .Add("floors/b.json", Floor.Replace("\"id\":\"a\"", "\"id\":\"b\"", StringComparison.Ordinal).Replace("\"minDepth\":1,\"maxDepth\":5", "\"minDepth\":6,\"maxDepth\":9", StringComparison.Ordinal))
            .Add("floors/a.json", Floor)
            .Add("chambers/b.json", ChamberKindText.Replace("\"k\"", "\"b\"", StringComparison.Ordinal))
            .Add("chambers/a.json", ChamberKindText.Replace("\"k\"", "\"a\"", StringComparison.Ordinal));

        ContentSet first = new ContentLoader(forward).Load();
        ContentSet second = new ContentLoader(backward).Load();
        Assert.Equal(first.Hash, second.Hash);
        Assert.Equal("a", first.Chambers[0].Id);
        Assert.Equal("b", first.Chambers[1].Id);
        Assert.Equal("a", second.Chambers[0].Id);
        Assert.Equal("b", second.Chambers[1].Id);
        Assert.Equal("a", first.Floors[0].Id);
        Assert.Equal("b", first.Floors[1].Id);
        Assert.Equal("a", second.Floors[0].Id);
        Assert.Equal("b", second.Floors[1].Id);
    }

    private const string ProjectileText = """
        {"id":"p","speedCentimetres":4000,"gravityScalePercent":100,"lifetimeTicks":300,"damage":10,"spreadHundredths":100}
        """;

    /// <summary>Every required field of a projectile definition is required, the spread among them (D-266).</summary>
    [Theory]
    [InlineData("id")]
    [InlineData("speedCentimetres")]
    [InlineData("gravityScalePercent")]
    [InlineData("lifetimeTicks")]
    [InlineData("damage")]
    [InlineData("spreadHundredths")]
    public void EveryRequiredProjectileFieldIsRequired(string omitted)
    {
        List<JsonMember> members = [];
        foreach (JsonMember member in JsonObjectReader.Read("projectiles/p.json", Encoding.UTF8.GetBytes(ProjectileText)))
        {
            if (member.Name != omitted)
            {
                members.Add(member);
            }
        }

        ContextException error = Assert.Throws<ContextException>(() => ProjectileDefinition.FromMembers("projectiles/p.json", members));
        Assert.Contains(omitted, error.Message, StringComparison.Ordinal);
    }

    /// <summary>A projectile definition outside its bounds is an error that names the field: no speed, a lifting gravity, a spread past a half turn (D-266).</summary>
    [Theory]
    [InlineData("\"speedCentimetres\":4000", "\"speedCentimetres\":0", "speedCentimetres")]
    [InlineData("\"gravityScalePercent\":100", "\"gravityScalePercent\":-1", "gravityScalePercent")]
    [InlineData("\"spreadHundredths\":100", "\"spreadHundredths\":-1", "spreadHundredths")]
    [InlineData("\"spreadHundredths\":100", "\"spreadHundredths\":18001", "spreadHundredths")]
    [InlineData("\"lifetimeTicks\":300", "\"lifetimeTicks\":0", "lifetimeTicks")]
    public void AProjectileOutsideItsBoundsIsAnError(string from, string to, string field)
    {
        string text = ProjectileText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(ProjectileText, text);

        ContextException error = Assert.Throws<ContextException>(
            () => ProjectileDefinition.FromMembers("projectiles/p.json", JsonObjectReader.Read("projectiles/p.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }
}
