using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WhatYouCarry.Core.Combat;
using WhatYouCarry.Core.Content;
using WhatYouCarry.Core.Entities;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Procgen;
using WhatYouCarry.Core.Simulation;
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
        {"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine","sizeX":48,"sizeY":12,"sizeZ":48,"galleryWidth":7,"galleryHeight":5,"driftWidth":5,"driftHeight":4,"chamberHeightMin":5,"chamberHeightMax":8,"rampSlopeRuns":[2,3,4],"timerSeconds":180,"bossTimerSeconds":120,"waveIntervalSeconds":30,"waveCap":12}
        """;

    private const string ChamberKindText = """
        {"id":"k","weight":10,"boxCountMin":1,"boxCountMax":3,"boxSizeMin":3,"boxSizeMax":6,"tierChance":25}
        """;

    private const string StringTable = """
        {"hub.descend":"Descend"}
        """;

    private const string HunterText = """
        {"id":"h","weapon":"w","attackRangeCentimetres":180,"attackCooldownTicks":60,"startSpeedCentimetresPerSecond":350,"speedGainCentimetresPerSecond":100,"speedGainTicks":1200}
        """;

    private static MemorySource Valid() => new MemorySource()
        .Add("floors/a.json", Floor)
        .Add("weapons/w.json", WeaponText)
        .Add("hunter/h.json", HunterText)
        .Add(Strings.FilePath, StringTable);

    /// <summary>PR-5 exit test 1. An absent field names the file, the field, and the reason (D-92, T-2).</summary>
    [Fact]
    public void AbsentFieldNamesField()
    {
        MemorySource source = new MemorySource()
            .Add("floors/a.json", """{"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"band":"working-mine","sizeX":48,"sizeY":12,"sizeZ":48,"galleryWidth":7,"galleryHeight":5,"driftWidth":5,"driftHeight":4,"chamberHeightMin":5,"chamberHeightMax":8,"rampSlopeRuns":[2,3,4],"timerSeconds":180,"bossTimerSeconds":120,"waveIntervalSeconds":30,"waveCap":12}""")
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
    [InlineData("galleryWidth")]
    [InlineData("galleryHeight")]
    [InlineData("driftWidth")]
    [InlineData("driftHeight")]
    [InlineData("chamberHeightMin")]
    [InlineData("chamberHeightMax")]
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
            .Add("floors/a.json", """{"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine","sizeX":48,"sizeY":12,"sizeZ":48,"galleryWidth":7,"galleryHeight":5,"driftWidth":5,"driftHeight":4,"chamberHeightMin":5,"chamberHeightMax":8,"rampSlopeRuns":[2,3,4],"timerSeconds":180,"bossTimerSeconds":120,"waveIntervalSeconds":30,"waveCap":12,"extra":1}""")
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
        Assert.Equal(2, set.Weapons.Count);
        Assert.Equal("overseer-pick", set.Weapons[0].Id);
        Assert.Equal("sword-basic", set.Weapons[1].Id);
        Assert.Equal("overseer", set.Hunter.Id);
        Assert.Equal("overseer-pick", set.Hunter.Weapon);
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

    /// <summary>
    /// F-131. A floor band outside the three bands of D-210 fails at load and names the field (G-7). The old loader took
    /// any text, and the detail pass failed only when a floor of the band was dug. Each of the three bands loads.
    /// </summary>
    [Fact]
    public void AnUnknownBandFailsAtLoad()
    {
        string odd = Floor.Replace("\"band\":\"working-mine\"", "\"band\":\"sunlit-meadow\"", StringComparison.Ordinal);
        Assert.NotEqual(Floor, odd);
        ContextException error = Assert.Throws<ContextException>(
            () => FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(odd))));
        Assert.Contains("floors/a.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("'band'", error.Message, StringComparison.Ordinal);
        Assert.Contains("sunlit-meadow", error.Message, StringComparison.Ordinal);

        foreach (string band in FloorTemplate.Bands)
        {
            string text = Floor.Replace("\"band\":\"working-mine\"", $"\"band\":\"{band}\"", StringComparison.Ordinal);
            Assert.Equal(band, FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(text))).Band);
        }

        Assert.Equal(FloorTemplate.WorkingMineBand, Core.Procgen.DetailPass.WorkingMine);
        Assert.Equal(FloorTemplate.OlderWorkingsBand, Core.Procgen.DetailPass.OlderWorkings);
        Assert.Equal(FloorTemplate.DeepBand, Core.Procgen.DetailPass.Deep);
    }

    /// <summary>F-131. An empty string value fails at load and names its id, because it shows the player nothing (D-98, T-2). A value of one space still loads.</summary>
    [Fact]
    public void AnEmptyStringFailsAtLoad()
    {
        ContextException error = Assert.Throws<ContextException>(
            () => Strings.FromMembers("strings/en.json", JsonObjectReader.Read("strings/en.json", Encoding.UTF8.GetBytes("{\"hub.descend\":\"Descend\",\"hub.empty\":\"\"}"))));
        Assert.Contains("strings/en.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("hub.empty", error.Message, StringComparison.Ordinal);

        Strings spaced = Strings.FromMembers("strings/en.json", JsonObjectReader.Read("strings/en.json", Encoding.UTF8.GetBytes("{\"hub.space\":\" \"}")));
        Assert.Equal(" ", spaced.Get("hub.space"));
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
            .Add("weapons/w.json", WeaponText)
            .Add("hunter/h.json", HunterText)
            .Add("floors/a.json", Floor.Replace("100", "101", StringComparison.Ordinal))
            .Add(Strings.FilePath, StringTable);
        Assert.NotEqual(first, new ContentLoader(changed).Load().Hash);

        // The order of the source does not change the hash, and the path does.
        MemorySource reordered = new MemorySource()
            .Add("weapons/w.json", WeaponText)
            .Add("hunter/h.json", HunterText)
            .Add(Strings.FilePath, StringTable)
            .Add("floors/a.json", Floor);
        Assert.Equal(first, new ContentLoader(reordered).Load().Hash);

        MemorySource renamed = new MemorySource()
            .Add("weapons/w.json", WeaponText)
            .Add("hunter/h.json", HunterText)
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

    /// <summary>Two weapon definitions of one id are an error, because the loadout of PR-30 looks a weapon up by it (D-334).</summary>
    [Fact]
    public void ARepeatedWeaponIdIsAnError()
    {
        MemorySource source = Valid()
            .Add("weapons/a.json", WeaponText)
            .Add("weapons/b.json", WeaponText);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("two weapon definitions", error.Message, StringComparison.Ordinal);
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
            .Add("floors/a.json", """{"id":1,"minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine","sizeX":48,"sizeY":12,"sizeZ":48,"galleryWidth":7,"galleryHeight":5,"driftWidth":5,"driftHeight":4,"chamberHeightMin":5,"chamberHeightMax":8,"rampSlopeRuns":[2,3,4],"timerSeconds":180,"bossTimerSeconds":120,"waveIntervalSeconds":30,"waveCap":12}""")
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

    /// <summary>A list of numbers reads as one member with the item texts, which the floor template splits into its ramp slope runs (D-346).</summary>
    [Fact]
    public void AListOfNumbersReads()
    {
        IReadOnlyList<JsonMember> members = JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes("{\"rampSlopeRuns\":[2,3,4]}"));
        Assert.Equal(new JsonMember("rampSlopeRuns", "2,3,4", JsonMemberKind.NumberList), Assert.Single(members));
    }

    /// <summary>A list of another kind, a nested object, and a list that the file ends inside are each an error that names the field (D-229, D-346).</summary>
    [Theory]
    [InlineData("{\"loadout\":[[]]}", "not a number")]
    [InlineData("{\"loadout\":[\"a\"]}", "not a number")]
    [InlineData("{\"loadout\":{}}", "no Phase 1 type uses")]
    [InlineData("{\"loadout\":[", "not valid JSON")]
    public void AListOfAnotherKindIsAnError(string text, string reason)
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
    [InlineData("tierChance")]
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
    [InlineData("\"sizeY\":12", "\"sizeY\":5", "sizeY")]
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

    /// <summary>
    /// PR-63 exit test 3. On a floor of 48 by 12 by 48, a dig size under the three blocks of D-166, an even tunnel width,
    /// a width past the shell, a height past the rows of the floor, and a chamber height range upside down are each an
    /// error that names the field and the reason (D-342, D-352).
    /// </summary>
    [Theory]
    [InlineData("\"galleryWidth\":7", "\"galleryWidth\":1", "galleryWidth", "is from 3 to 46")]
    [InlineData("\"galleryWidth\":7", "\"galleryWidth\":6", "galleryWidth", "is odd")]
    [InlineData("\"galleryWidth\":7", "\"galleryWidth\":47", "galleryWidth", "is from 3 to 46")]
    [InlineData("\"galleryHeight\":5", "\"galleryHeight\":2", "galleryHeight", "is from 3 to 9")]
    [InlineData("\"driftWidth\":5", "\"driftWidth\":2", "driftWidth", "is from 3 to 46")]
    [InlineData("\"driftWidth\":5", "\"driftWidth\":4", "driftWidth", "is odd")]
    [InlineData("\"driftHeight\":4", "\"driftHeight\":2", "driftHeight", "is from 3 to 9")]
    [InlineData("\"driftHeight\":4", "\"driftHeight\":10", "driftHeight", "is from 3 to 9")]
    [InlineData("\"chamberHeightMin\":5", "\"chamberHeightMin\":2", "chamberHeightMin", "is from 3 to 9")]
    [InlineData("\"chamberHeightMax\":8", "\"chamberHeightMax\":10", "chamberHeightMax", "is from 3 to 9")]
    [InlineData("\"chamberHeightMin\":5", "\"chamberHeightMin\":9", "chamberHeightMin", "is above chamberHeightMax")]
    public void ADigSizeOutsideItsBoundsIsAnError(string from, string to, string field, string reason)
    {
        string text = Floor.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(Floor, text);

        ContextException error = Assert.Throws<ContextException>(
            () => FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
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

    /// <summary>
    /// The chamber kinds of the checkout load with their weights and the box ranges of D-341. Every floor template
    /// carries the one floor size of D-343, the dig sizes of D-341, and the room count and budget of D-344 (D-255).
    /// </summary>
    [Fact]
    public void TheChamberKindsAndFloorSizesLoad()
    {
        ContentSet set = TestWorld.Content;

        Dictionary<string, (int Min, int Max)> boxSizes = new()
        {
            ["crosscut-junction"] = (5, 8),
            ["stope"] = (6, 11),
            ["cavern"] = (8, 14),
            ["great-stope"] = (9, 15),
            ["lamp-room"] = (5, 8),
            ["ore-bin"] = (8, 12),
            ["powder-magazine"] = (5, 8),
            ["pump-chamber"] = (9, 12),
        };
        long lightest = long.MaxValue;
        long heaviest = 0;
        foreach (ChamberKind kind in set.Chambers)
        {
            lightest = Math.Min(lightest, kind.Weight);
            heaviest = Math.Max(heaviest, kind.Weight);
            Assert.True(kind.BoxSizeMin >= ChamberKind.SmallestBoxSide, $"The kind '{kind.Id}' has a box side of {kind.BoxSizeMin}, below the tunnel width.");
            Assert.True(boxSizes.TryGetValue(kind.Id, out (int Min, int Max) sizes), $"The kind '{kind.Id}' has no box range in D-341.");
            Assert.True(kind.BoxSizeMin == sizes.Min && kind.BoxSizeMax == sizes.Max, $"The kind '{kind.Id}' has boxes of {kind.BoxSizeMin} to {kind.BoxSizeMax}, and D-341 names {sizes.Min} to {sizes.Max}.");
        }

        Assert.Equal(8, lightest);
        Assert.Equal(40, heaviest);
        Assert.Equal(boxSizes.Count, set.Chambers.Count);

        foreach (FloorTemplate floor in set.Floors)
        {
            string context = $"The template '{floor.Id}'";
            Assert.True(floor.SizeX == 64 && floor.SizeY == 20 && floor.SizeZ == 64, $"{context} is {floor.SizeX} by {floor.SizeY} by {floor.SizeZ}.");
            Assert.True(floor.GalleryWidth == 7 && floor.GalleryHeight == 5, $"{context} has a gallery of {floor.GalleryWidth} by {floor.GalleryHeight}.");
            Assert.True(floor.DriftWidth == 5 && floor.DriftHeight == 4, $"{context} has a drift of {floor.DriftWidth} by {floor.DriftHeight}.");
            Assert.True(floor.ChamberHeightMin == 5 && floor.ChamberHeightMax == 8, $"{context} has chambers {floor.ChamberHeightMin} to {floor.ChamberHeightMax} high.");
            Assert.True(floor.RoomCountMin == 5 && floor.RoomCountMax == 9 && floor.DifficultyBudget == 100, $"{context} holds {floor.RoomCountMin} to {floor.RoomCountMax} rooms with a budget of {floor.DifficultyBudget}.");
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
            .Add("weapons/w.json", WeaponText)
            .Add("hunter/h.json", HunterText)
            .Add("chambers/a.json", ChamberKindText.Replace("\"k\"", "\"a\"", StringComparison.Ordinal))
            .Add("chambers/b.json", ChamberKindText.Replace("\"k\"", "\"b\"", StringComparison.Ordinal))
            .Add("floors/a.json", Floor)
            .Add("floors/b.json", Floor.Replace("\"id\":\"a\"", "\"id\":\"b\"", StringComparison.Ordinal).Replace("\"minDepth\":1,\"maxDepth\":5", "\"minDepth\":6,\"maxDepth\":9", StringComparison.Ordinal))
            .Add(Strings.FilePath, StringTable);
        MemorySource backward = new MemorySource()
            .Add("weapons/w.json", WeaponText)
            .Add("hunter/h.json", HunterText)
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

    /// <summary>
    /// A projectile definition outside its bounds is an error that names the field: no speed, a lifting gravity, a
    /// spread past a half turn (D-266), no damage, and a negative area (F-120).
    /// </summary>
    [Theory]
    [InlineData("\"speedCentimetres\":4000", "\"speedCentimetres\":0", "speedCentimetres")]
    [InlineData("\"gravityScalePercent\":100", "\"gravityScalePercent\":-1", "gravityScalePercent")]
    [InlineData("\"spreadHundredths\":100", "\"spreadHundredths\":-1", "spreadHundredths")]
    [InlineData("\"spreadHundredths\":100", "\"spreadHundredths\":18001", "spreadHundredths")]
    [InlineData("\"lifetimeTicks\":300", "\"lifetimeTicks\":0", "lifetimeTicks")]
    [InlineData("\"damage\":10", "\"damage\":0", "damage")]
    [InlineData("\"damage\":10", "\"damage\":10,\"areaCentimetres\":-1", "areaCentimetres")]
    public void AProjectileOutsideItsBoundsIsAnError(string from, string to, string field)
    {
        string text = ProjectileText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(ProjectileText, text);

        ContextException error = Assert.Throws<ContextException>(
            () => ProjectileDefinition.FromMembers("projectiles/p.json", JsonObjectReader.Read("projectiles/p.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>F-120. A damage of one and a present area of zero are the lowest values that load, and an absent area stays zero.</summary>
    [Fact]
    public void AProjectileAtItsLowestBoundsLoads()
    {
        string text = ProjectileText.Replace("\"damage\":10", "\"damage\":1,\"areaCentimetres\":0", StringComparison.Ordinal);
        Assert.NotEqual(ProjectileText, text);
        ProjectileDefinition lowest = ProjectileDefinition.FromMembers("projectiles/p.json", JsonObjectReader.Read("projectiles/p.json", Encoding.UTF8.GetBytes(text)));
        Assert.Equal(1, lowest.Damage);
        Assert.Equal(0, lowest.AreaCentimetres);

        ProjectileDefinition absent = ProjectileDefinition.FromMembers("projectiles/p.json", JsonObjectReader.Read("projectiles/p.json", Encoding.UTF8.GetBytes(ProjectileText)));
        Assert.Equal(0, absent.AreaCentimetres);
    }

    /// <summary>Two projectile definitions of one id are an error, because a lookup would take either one.</summary>
    [Fact]
    public void ARepeatedProjectileIdIsAnError()
    {
        MemorySource source = Valid()
            .Add("projectiles/a.json", ProjectileText)
            .Add("projectiles/b.json", ProjectileText);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("two projectile definitions", error.Message, StringComparison.Ordinal);
        Assert.Contains("'p'", error.Message, StringComparison.Ordinal);
    }

    private const string WeaponText = """
        {"id":"w","tier":0,"handedness":"one","windupTicks":12,"activeTicks":6,"recoveryTicks":18,"damage":34,"reachCentimetres":160,"arcHundredths":9000,"lowCentimetres":50,"highCentimetres":170,"model":"models/w.bbmodel","animation":"models/player.w.json"}
        """;

    /// <summary>Every field of a weapon definition is required (D-92, D-334).</summary>
    [Theory]
    [InlineData("id")]
    [InlineData("tier")]
    [InlineData("handedness")]
    [InlineData("windupTicks")]
    [InlineData("activeTicks")]
    [InlineData("recoveryTicks")]
    [InlineData("damage")]
    [InlineData("reachCentimetres")]
    [InlineData("arcHundredths")]
    [InlineData("lowCentimetres")]
    [InlineData("highCentimetres")]
    [InlineData("model")]
    [InlineData("animation")]
    public void EveryRequiredWeaponFieldIsRequired(string omitted)
    {
        List<JsonMember> members = [];
        foreach (JsonMember member in JsonObjectReader.Read("weapons/w.json", Encoding.UTF8.GetBytes(WeaponText)))
        {
            if (member.Name != omitted)
            {
                members.Add(member);
            }
        }

        ContextException error = Assert.Throws<ContextException>(() => WeaponDefinition.FromMembers("weapons/w.json", members));
        Assert.Contains(omitted, error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A weapon definition outside its bounds is an error that names the field: a negative tier, an unknown handedness, a
    /// phase of no tick or past the tick counter, no damage, no reach, an arc past a half turn, a band with no height,
    /// a path outside the model directory, a path with an empty, dot, or backslash segment, and a file name without the
    /// extension of its kind (D-26, D-219, D-298, D-325, D-334; PR #62 review P2-1).
    /// </summary>
    [Theory]
    [InlineData("\"tier\":0", "\"tier\":-1", "tier")]
    [InlineData("\"handedness\":\"one\"", "\"handedness\":\"three\"", "handedness")]
    [InlineData("\"windupTicks\":12", "\"windupTicks\":0", "windupTicks")]
    [InlineData("\"activeTicks\":6", "\"activeTicks\":4294967296", "activeTicks")]
    [InlineData("\"recoveryTicks\":18", "\"recoveryTicks\":-18", "recoveryTicks")]
    [InlineData("\"damage\":34", "\"damage\":0", "damage")]
    [InlineData("\"reachCentimetres\":160", "\"reachCentimetres\":0", "reachCentimetres")]
    [InlineData("\"arcHundredths\":9000", "\"arcHundredths\":18001", "arcHundredths")]
    [InlineData("\"arcHundredths\":9000", "\"arcHundredths\":0", "arcHundredths")]
    [InlineData("\"arcHundredths\":9000", "\"arcHundredths\":5", "arcHundredths")]
    [InlineData("\"lowCentimetres\":50", "\"lowCentimetres\":-1", "lowCentimetres")]
    [InlineData("\"highCentimetres\":170", "\"highCentimetres\":50", "highCentimetres")]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"w.bbmodel\"", "model")]
    [InlineData("\"animation\":\"models/player.w.json\"", "\"animation\":\"player.w.json\"", "animation")]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"models/../floors/a.json\"", "model")]
    [InlineData("\"animation\":\"models/player.w.json\"", "\"animation\":\"models/player.bbmodel\"", "animation")]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"models/player.w.json\"", "model")]
    [InlineData("\"animation\":\"models/player.w.json\"", "\"animation\":\"models/../player.w.json\"", "animation")]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"models/./w.bbmodel\"", "model")]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"models//w.bbmodel\"", "model")]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"models/a\\\\..\\\\w.bbmodel\"", "model")]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"models/.bbmodel\"", "model")]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"models/w.BBMODEL\"", "model")]
    public void AWeaponOutsideItsBoundsIsAnError(string from, string to, string field)
    {
        // The case of 5 hundredths over 6 active ticks is the arc with a step of no turn (PR #62 automated pass).
        string text = WeaponText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(WeaponText, text);

        ContextException error = Assert.Throws<ContextException>(
            () => WeaponDefinition.FromMembers("weapons/w.json", JsonObjectReader.Read("weapons/w.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>An arc of one hundredth of a degree for each active tick is the smallest arc that loads, and its blade turns on every step (D-325).</summary>
    [Fact]
    public void TheSmallestArcTurnsOnEveryActiveTick()
    {
        string text = WeaponText.Replace("\"arcHundredths\":9000", "\"arcHundredths\":6", StringComparison.Ordinal);
        WeaponDefinition weapon = WeaponDefinition.FromMembers("weapons/w.json", JsonObjectReader.Read("weapons/w.json", Encoding.UTF8.GetBytes(text)));
        for (int step = 0; step < weapon.ActiveTicks; step++)
        {
            Assert.True(MeleeWeapon.BladeOffset(weapon, step + 1) > MeleeWeapon.BladeOffset(weapon, step), $"The blade does not turn on step {step}.");
        }
    }

    /// <summary>A model path and an animation path under the model directory load, in a subdirectory of it too (D-298, D-334; PR #62 review P2-1).</summary>
    [Theory]
    [InlineData("\"model\":\"models/w.bbmodel\"", "\"model\":\"models/weapons/w.bbmodel\"")]
    [InlineData("\"animation\":\"models/player.w.json\"", "\"animation\":\"models/weapons/player.w.json\"")]
    public void AWeaponAssetPathUnderTheModelDirectoryLoads(string from, string to)
    {
        string text = WeaponText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(WeaponText, text);

        WeaponDefinition weapon = WeaponDefinition.FromMembers("weapons/w.json", JsonObjectReader.Read("weapons/w.json", Encoding.UTF8.GetBytes(text)));
        Assert.StartsWith(WeaponDefinition.AssetDirectory, weapon.Model, StringComparison.Ordinal);
        Assert.StartsWith(WeaponDefinition.AssetDirectory, weapon.Animation, StringComparison.Ordinal);
    }

    /// <summary>A content set holds exactly one hunter, and the hunter names a weapon of the set (D-45, D-56, D-413).</summary>
    [Fact]
    public void TheSetHoldsOneHunterWithAWeapon()
    {
        ContentSet set = new ContentLoader(Valid()).Load();
        Assert.Equal(new HunterDefinition("h", "w", 180, 60, 350, 100, 1200), set.Hunter);

        MemorySource none = new MemorySource()
            .Add("floors/a.json", Floor)
            .Add(Strings.FilePath, StringTable);
        ContextException noHunter = Assert.Throws<ContextException>(() => new ContentLoader(none).Load());
        Assert.Contains("hunters=0", noHunter.Message, StringComparison.Ordinal);

        ContextException twoHunters = Assert.Throws<ContextException>(() => new ContentLoader(Valid().Add("hunter/i.json", HunterText.Replace("\"h\"", "\"i\"", StringComparison.Ordinal))).Load());
        Assert.Contains("hunters=2", twoHunters.Message, StringComparison.Ordinal);

        MemorySource noWeapon = new MemorySource()
            .Add("floors/a.json", Floor)
            .Add("hunter/h.json", HunterText)
            .Add(Strings.FilePath, StringTable);
        ContextException missing = Assert.Throws<ContextException>(() => new ContentLoader(noWeapon).Load());
        Assert.Contains("D-413", missing.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Every field of the hunter is required, and each number is one or more (D-92, D-408). The cooldown fits the int
    /// that the hunter counts it in: the loader took 2^31, one past <see cref="int.MaxValue"/>, and the cast wrapped it
    /// to a negative cooldown, which never ends (F-120).
    /// </summary>
    [Theory]
    [InlineData("\"attackRangeCentimetres\":180", "\"attackRangeCentimetres\":0", "attackRangeCentimetres")]
    [InlineData("\"attackCooldownTicks\":60", "\"attackCooldownTicks\":0", "attackCooldownTicks")]
    [InlineData("\"startSpeedCentimetresPerSecond\":350", "\"startSpeedCentimetresPerSecond\":0", "startSpeedCentimetresPerSecond")]
    [InlineData("\"speedGainCentimetresPerSecond\":100", "\"speedGainCentimetresPerSecond\":0", "speedGainCentimetresPerSecond")]
    [InlineData("\"speedGainTicks\":1200", "\"speedGainTicks\":0", "speedGainTicks")]
    [InlineData(",\"speedGainTicks\":1200", "", "speedGainTicks")]
    [InlineData("\"attackCooldownTicks\":60", "\"attackCooldownTicks\":2147483648", "attackCooldownTicks")]
    public void ABadHunterFieldFails(string from, string to, string field)
    {
        string text = HunterText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(HunterText, text);
        ContextException error = Assert.Throws<ContextException>(
            () => HunterDefinition.FromMembers("hunter/h.json", JsonObjectReader.Read("hunter/h.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
    }

    /// <summary>The timer and wave fields of a floor template are required and bounded (D-407, D-410).</summary>
    [Theory]
    [InlineData("\"timerSeconds\":180", "\"timerSeconds\":0", "timerSeconds")]
    [InlineData("\"bossTimerSeconds\":120", "\"bossTimerSeconds\":-1", "bossTimerSeconds")]
    [InlineData("\"waveIntervalSeconds\":30", "\"waveIntervalSeconds\":0", "waveIntervalSeconds")]
    [InlineData("\"waveCap\":12", "\"waveCap\":-1", "waveCap")]
    [InlineData(",\"waveCap\":12", "", "waveCap")]
    public void ABadTimerFieldFails(string from, string to, string field)
    {
        string text = Floor.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(Floor, text);
        ContextException error = Assert.Throws<ContextException>(
            () => FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// F-120. A floor value past what its consumer holds is an error that names the field. The loader took each one,
    /// and a cast or a product wrapped it later. Each row holds the first value past the bound: 2^31, one past
    /// <see cref="int.MaxValue"/>, for the floor number; for the timer and the wave interval, the first count of
    /// seconds whose ticks, at 60 a second, pass <see cref="long.MaxValue"/>; and for the extra seconds of a boss
    /// floor, the first count whose sum with the 180 seconds of the timer passes that bound.
    /// </summary>
    [Theory]
    [InlineData("\"maxDepth\":5", "\"maxDepth\":2147483648", "maxDepth")]
    [InlineData("\"timerSeconds\":180", "\"timerSeconds\":153722867280912931", "timerSeconds")]
    [InlineData("\"bossTimerSeconds\":120", "\"bossTimerSeconds\":153722867280912751", "bossTimerSeconds")]
    [InlineData("\"waveIntervalSeconds\":30", "\"waveIntervalSeconds\":153722867280912931", "waveIntervalSeconds")]
    public void AFloorValuePastWhatItsConsumerHoldsIsAnError(string from, string to, string field)
    {
        string text = Floor.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(Floor, text);
        ContextException error = Assert.Throws<ContextException>(
            () => FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains("floors/a.json", error.Message, StringComparison.Ordinal);
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// F-120. The last floor value that each consumer holds loads, and the loop reads it with no wrap: the deepest floor
    /// is <see cref="int.MaxValue"/>, and a timer of the most seconds, alone or with the extra seconds of a boss floor,
    /// runs <see cref="long.MaxValue"/> ticks rounded down to a whole second.
    /// </summary>
    [Fact]
    public void AFloorValueAtWhatItsConsumerHoldsLoads()
    {
        long mostSeconds = long.MaxValue / SimulationLoop.TicksPerSecond;
        string bossText = Floor
            .Replace("\"maxDepth\":5", $"\"maxDepth\":{int.MaxValue}", StringComparison.Ordinal)
            .Replace("\"bossTimerSeconds\":120", $"\"bossTimerSeconds\":{mostSeconds - 180}", StringComparison.Ordinal)
            .Replace("\"waveIntervalSeconds\":30", $"\"waveIntervalSeconds\":{mostSeconds}", StringComparison.Ordinal);
        FloorTemplate boss = FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(bossText)));
        Assert.Equal(int.MaxValue, FloorGenerator.DeepestFloor(TestWorld.Content with { Floors = [boss] }));
        Assert.Equal(mostSeconds * SimulationLoop.TicksPerSecond, FloorTimer.For(boss, FloorTimer.BossFloors[0]).Length);
        Escalation waves = new(boss);
        long intervalTicks = mostSeconds * SimulationLoop.TicksPerSecond;
        Assert.Equal(0, waves.Step(TestWorld.FlatFloor(), intervalTicks - 1, 0, [], TestWorld.Spawn).Wave);
        Assert.Equal(1, waves.Step(TestWorld.FlatFloor(), intervalTicks, 0, [], TestWorld.Spawn).Wave);

        string timerText = Floor
            .Replace("\"timerSeconds\":180", $"\"timerSeconds\":{mostSeconds}", StringComparison.Ordinal)
            .Replace("\"bossTimerSeconds\":120", "\"bossTimerSeconds\":0", StringComparison.Ordinal);
        FloorTemplate longest = FloorTemplate.FromMembers("floors/a.json", JsonObjectReader.Read("floors/a.json", Encoding.UTF8.GetBytes(timerText)));
        Assert.Equal(mostSeconds * SimulationLoop.TicksPerSecond, FloorTimer.For(longest, 1).Length);
    }

    /// <summary>F-120. The limits that Core writes as numbers, because its member allowlist holds no MaxValue of int or long, are those members.</summary>
    [Fact]
    public void TheWrittenLimitsAreTheLimitsOfTheirTypes()
    {
        Assert.Equal(int.MaxValue, ContentValidator.LargestInt);
        Assert.Equal(long.MaxValue, ContentValidator.LargestLong);
        Assert.Equal(long.MaxValue / SimulationLoop.TicksPerSecond, FloorTemplate.LargestSeconds);
    }

    private const string EnemyText = """
        {"id":"e","minDepth":1,"maxDepth":5,"weight":10,"health":40,"weapon":"w","sightCentimetres":2000,"giveUpTicks":300,"attackRangeCentimetres":140,"attackCooldownTicks":30,"speedCentimetresPerSecond":500}
        """;

    /// <summary>Every field of an enemy family is required, one at a time (D-92, D-395).</summary>
    [Theory]
    [InlineData("id")]
    [InlineData("minDepth")]
    [InlineData("maxDepth")]
    [InlineData("weight")]
    [InlineData("health")]
    [InlineData("weapon")]
    [InlineData("sightCentimetres")]
    [InlineData("giveUpTicks")]
    [InlineData("attackRangeCentimetres")]
    [InlineData("attackCooldownTicks")]
    [InlineData("speedCentimetresPerSecond")]
    public void EveryRequiredEnemyFieldIsRequired(string omitted)
    {
        List<JsonMember> members = [];
        foreach (JsonMember member in JsonObjectReader.Read("enemies/e.json", Encoding.UTF8.GetBytes(EnemyText)))
        {
            if (member.Name != omitted)
            {
                members.Add(member);
            }
        }

        ContextException error = Assert.Throws<ContextException>(() => EnemyDefinition.FromMembers("enemies/e.json", members));
        Assert.Contains($"'{omitted}'", error.Message, StringComparison.Ordinal);
        Assert.Contains("is absent", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// An enemy family outside its bounds is an error that names the field: a first floor below 1, a range upside down,
    /// no weight, no health, a number that no whole number holds, a field of another kind, and a distance, a delay, or
    /// a speed of zero (D-3, D-167, D-322, D-395).
    /// </summary>
    [Theory]
    [InlineData("\"minDepth\":1", "\"minDepth\":0", "minDepth")]
    [InlineData("\"maxDepth\":5", "\"maxDepth\":0", "maxDepth")]
    [InlineData("\"weight\":10", "\"weight\":0", "weight")]
    [InlineData("\"health\":40", "\"health\":0", "health")]
    [InlineData("\"health\":40", "\"health\":1.5", "health")]
    [InlineData("\"weapon\":\"w\"", "\"weapon\":1", "weapon")]
    [InlineData("\"sightCentimetres\":2000", "\"sightCentimetres\":0", "sightCentimetres")]
    [InlineData("\"giveUpTicks\":300", "\"giveUpTicks\":0", "giveUpTicks")]
    [InlineData("\"attackRangeCentimetres\":140", "\"attackRangeCentimetres\":0", "attackRangeCentimetres")]
    [InlineData("\"attackCooldownTicks\":30", "\"attackCooldownTicks\":-1", "attackCooldownTicks")]
    [InlineData("\"speedCentimetresPerSecond\":500", "\"speedCentimetresPerSecond\":0", "speedCentimetresPerSecond")]
    public void AnEnemyOutsideItsBoundsIsAnError(string from, string to, string field)
    {
        string text = EnemyText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(EnemyText, text);
        ContextException error = Assert.Throws<ContextException>(
            () => EnemyDefinition.FromMembers("enemies/e.json", JsonObjectReader.Read("enemies/e.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains("enemies/e.json", error.Message, StringComparison.Ordinal);
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// F-120. An enemy value past the int that an enemy holds it in is an error that names the field. The loader took
    /// 2^31, one past <see cref="int.MaxValue"/>: a cast wrapped the health to -2^31 and the cooldown to a negative
    /// count that never ends, and the int count of blind ticks wraps before it reaches that give-up count.
    /// </summary>
    [Theory]
    [InlineData("\"health\":40", "\"health\":2147483648", "health")]
    [InlineData("\"giveUpTicks\":300", "\"giveUpTicks\":2147483648", "giveUpTicks")]
    [InlineData("\"attackCooldownTicks\":30", "\"attackCooldownTicks\":2147483648", "attackCooldownTicks")]
    public void AnEnemyValuePastAnIntIsAnError(string from, string to, string field)
    {
        string text = EnemyText.Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(EnemyText, text);
        ContextException error = Assert.Throws<ContextException>(
            () => EnemyDefinition.FromMembers("enemies/e.json", JsonObjectReader.Read("enemies/e.json", Encoding.UTF8.GetBytes(text))));
        Assert.Contains("enemies/e.json", error.Message, StringComparison.Ordinal);
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// F-120. An enemy and a hunter at <see cref="int.MaxValue"/> load, and the spawn holds the health and the cooldown
    /// with no wrap.
    /// </summary>
    [Fact]
    public void AnEnemyAndAHunterAtTheIntLimitLoad()
    {
        string enemyText = EnemyText
            .Replace("\"health\":40", $"\"health\":{int.MaxValue}", StringComparison.Ordinal)
            .Replace("\"giveUpTicks\":300", $"\"giveUpTicks\":{int.MaxValue}", StringComparison.Ordinal)
            .Replace("\"attackCooldownTicks\":30", $"\"attackCooldownTicks\":{int.MaxValue}", StringComparison.Ordinal);
        EnemyDefinition family = EnemyDefinition.FromMembers("enemies/e.json", JsonObjectReader.Read("enemies/e.json", Encoding.UTF8.GetBytes(enemyText)));
        Assert.Equal(int.MaxValue, family.GiveUpTicks);
        Assert.Equal(int.MaxValue, family.AttackCooldownTicks);
        WeaponDefinition weapon = WeaponDefinition.FromMembers("weapons/w.json", JsonObjectReader.Read("weapons/w.json", Encoding.UTF8.GetBytes(WeaponText)));
        Enemy enemy = new(TestWorld.FlatFloor(), TestWorld.Spawn, family, weapon, 1);
        Assert.Equal(int.MaxValue, enemy.Health);

        string hunterText = HunterText.Replace("\"attackCooldownTicks\":60", $"\"attackCooldownTicks\":{int.MaxValue}", StringComparison.Ordinal);
        HunterDefinition hunter = HunterDefinition.FromMembers("hunter/h.json", JsonObjectReader.Read("hunter/h.json", Encoding.UTF8.GetBytes(hunterText)));
        Assert.Equal(int.MaxValue, hunter.AttackCooldownTicks);
    }

    /// <summary>An enemy family that names a weapon of the set loads, and one that names no weapon of the set is an error that names the family and the field (D-31, D-397).</summary>
    [Fact]
    public void AnEnemyNamesAWeaponOfTheSet()
    {
        ContentSet set = new ContentLoader(Valid().Add("enemies/e.json", EnemyText)).Load();
        Assert.Equal("w", Assert.Single(set.Enemies).Weapon);

        MemorySource unknown = Valid().Add("enemies/e.json", EnemyText.Replace("\"weapon\":\"w\"", "\"weapon\":\"x\"", StringComparison.Ordinal));
        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(unknown).Load());
        Assert.Contains("enemies/e", error.Message, StringComparison.Ordinal);
        Assert.Contains("'weapon'", error.Message, StringComparison.Ordinal);
        Assert.Contains("'x'", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-397", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Two enemy families of one id are an error, because the spawns look a family up by it (D-395).</summary>
    [Fact]
    public void ARepeatedEnemyIdIsAnError()
    {
        MemorySource source = Valid()
            .Add("enemies/a.json", EnemyText)
            .Add("enemies/b.json", EnemyText);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("two enemy families", error.Message, StringComparison.Ordinal);
        Assert.Contains("'e'", error.Message, StringComparison.Ordinal);
    }
}
