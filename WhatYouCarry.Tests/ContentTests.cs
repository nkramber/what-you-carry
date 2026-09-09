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
        {"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine"}
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
            .Add("floors/a.json", """{"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"band":"working-mine"}""")
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
            .Add("floors/a.json", """{"id":"a","minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine","extra":1}""")
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
        Assert.Equal(2, set.Projectiles.Count);
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
            .Add("floors/a.json", """{"id":1,"minDepth":1,"maxDepth":5,"roomCountMin":4,"roomCountMax":8,"difficultyBudget":100,"band":"working-mine"}""")
            .Add(Strings.FilePath, StringTable);

        ContextException error = Assert.Throws<ContextException>(() => new ContentLoader(source).Load());
        Assert.Contains("Number", error.Message, StringComparison.Ordinal);
        Assert.Contains("Text", error.Message, StringComparison.Ordinal);
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

    /// <summary>The content source of this checkout, which reads the real `content/` directory.</summary>
    private sealed class RepositoryContentSource : IContentSource
    {
        public IReadOnlyList<ContentFile> Read()
        {
            string root = Path.Combine(RepositoryRoot.Find(), "content");
            List<ContentFile> files = [];
            foreach (string file in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
            {
                files.Add(new ContentFile(Path.GetRelativePath(root, file).Replace('\\', '/'), File.ReadAllBytes(file)));
            }

            return files;
        }
    }
}
