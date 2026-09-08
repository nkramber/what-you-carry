using System;
using System.Collections.Generic;
using WhatYouCarry.Core.Logging;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The logger, the error context, and the assertions (D-68, D-112, D-113; PR-4 exit tests 1 to 6).</summary>
public sealed class LoggingTests
{
    /// <summary>A sink that keeps every line, which is the test half of the two callers that D-111 asks for.</summary>
    private sealed class CollectingSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }

    /// <summary>PR-4 exit test 1. A run line without a seed is an error, and the error names the field (T-2).</summary>
    [Fact]
    public void LogLineWithoutContextThrows()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);

        LogFields missing = RunFieldsExcept("seed");

        ContextException error = Assert.Throws<ContextException>(
            () => logger.Write(LogContextKind.Run, LogLevel.Error, "a projectile left the grid", missing));

        Assert.Contains("seed", error.Message, StringComparison.Ordinal);
        Assert.Contains("missingField", error.Message, StringComparison.Ordinal);
        Assert.Empty(sink.Lines);
    }

    /// <summary>Every required field of each context is required, one at a time (D-113).</summary>
    [Theory]
    [InlineData("seed")]
    [InlineData("floor")]
    [InlineData("tick")]
    [InlineData("subsystem")]
    [InlineData("entities")]
    public void EveryRequiredRunFieldIsRequired(string omitted)
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);

        LogFields fields = RunFieldsExcept(omitted);

        ContextException error = Assert.Throws<ContextException>(
            () => logger.Write(LogContextKind.Run, LogLevel.Info, "a message", fields));
        Assert.Contains(omitted, error.Message, StringComparison.Ordinal);
    }

    /// <summary>The hub context has its own required set, and a run field does not satisfy it (D-113).</summary>
    [Fact]
    public void TheHubContextHasItsOwnRequiredSet()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);

        Assert.Throws<ContextException>(
            () => logger.Write(LogContextKind.Hub, LogLevel.Warning, "a message", RunFields()));

        logger.Write(LogContextKind.Hub, LogLevel.Warning, "a message", HubFields());
        Assert.Single(sink.Lines);
    }

    /// <summary>PR-4 exit test 2. An emitted line parses, and it holds every required key and the level.</summary>
    [Fact]
    public void LogLineIsValidJson()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);
        logger.Write(LogContextKind.Run, LogLevel.Warning, "the hunter reached the player", RunFields());

        string line = Assert.Single(sink.Lines);
        Assert.DoesNotContain("\n", line, StringComparison.Ordinal);

        System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(line);
        System.Text.Json.JsonElement root = document.RootElement;
        Assert.Equal("warning", root.GetProperty("level").GetString());
        Assert.Equal("the hunter reached the player", root.GetProperty("message").GetString());
        Assert.Equal(20260908L, root.GetProperty("seed").GetInt64());
        Assert.Equal(3L, root.GetProperty("floor").GetInt64());
        Assert.Equal(1820L, root.GetProperty("tick").GetInt64());
        Assert.Equal("combat", root.GetProperty("subsystem").GetString());
        Assert.Equal(2, root.GetProperty("entities").GetArrayLength());

        foreach (string required in JsonlLogger.RequiredFields(LogContextKind.Run))
        {
            Assert.True(root.TryGetProperty(required, out _), $"the line has no field '{required}'");
        }
    }

    /// <summary>A value that holds a quotation mark, a backslash, or a line break stays on one line.</summary>
    [Fact]
    public void AValueWithControlCharactersStaysOnOneLine()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);

        LogFields fields = RunFields();
        fields.Add("path", "C:\\saves\\a\"b\nc\td\u0001e");
        logger.Write(LogContextKind.Run, LogLevel.Error, "a \"quoted\" message\nwith a break", fields);

        string line = Assert.Single(sink.Lines);
        Assert.DoesNotContain("\n", line, StringComparison.Ordinal);

        System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(line);
        Assert.Equal("C:\\saves\\a\"b\nc\td\u0001e", document.RootElement.GetProperty("path").GetString());
        Assert.Equal("a \"quoted\" message\nwith a break", document.RootElement.GetProperty("message").GetString());
    }

    /// <summary>A float value keeps enough digits to read back as the same float, on every machine.</summary>
    [Fact]
    public void AFloatValueReadsBackAsTheSameFloat()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);

        LogFields fields = RunFields();
        fields.Add("x", 1.23456789f);
        fields.Add("y", -0.0f);
        logger.Write(LogContextKind.Run, LogLevel.Debug, "a position", fields);

        System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(Assert.Single(sink.Lines));
        Assert.Equal(1.23456789f, float.Parse(document.RootElement.GetProperty("x").GetString()!, System.Globalization.CultureInfo.InvariantCulture));
        Assert.True(float.IsNegative(float.Parse(document.RootElement.GetProperty("y").GetString()!, System.Globalization.CultureInfo.InvariantCulture)));
    }

    /// <summary>A repeated field name is an error, because a JSON object with two equal names has no reading (T-2).</summary>
    [Fact]
    public void ARepeatedFieldNameIsAnError()
    {
        LogFields fields = new();
        fields.Add("seed", 1L);
        ContextException error = Assert.Throws<ContextException>(() => fields.Add("seed", 2L));
        Assert.Contains("seed", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A line with no message is an error, because a report with no text names nothing (T-2).</summary>
    [Fact]
    public void ALineWithNoMessageIsAnError()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);
        Assert.Throws<ContextException>(() => logger.Write(LogContextKind.Run, LogLevel.Info, string.Empty, RunFields()));
    }

    /// <summary>
    /// The logger writes the level and the message itself, so a caller field of either name is an error. Two
    /// fields of one name in a JSON object let a reader take either value (D-212, F-72).
    /// </summary>
    [Theory]
    [InlineData("level")]
    [InlineData("message")]
    [InlineData("assertFile")]
    [InlineData("assertLine")]
    [InlineData("assertMember")]
    public void AReservedFieldNameIsAnError(string reserved)
    {
        LogFields fields = new();
        ContextException error = Assert.Throws<ContextException>(() => fields.Add(reserved, "a value"));
        Assert.Contains(reserved, error.Message, StringComparison.Ordinal);

        // The rejection happens at the add, so no line ever reaches the sink with two of one name.
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);
        logger.Write(LogContextKind.Run, LogLevel.Error, "a message", RunFields());
        System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(Assert.Single(sink.Lines));
        int count = 0;
        foreach (System.Text.Json.JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (property.Name == reserved)
            {
                count++;
            }
        }

        // The logger writes the level and the message on every line. The three call-site names belong to the
        // assertion report, so an ordinary line carries none of them.
        int expected = reserved == LogFields.LevelName || reserved == LogFields.MessageName ? 1 : 0;
        Assert.Equal(expected, count);
    }

    /// <summary>
    /// A safe assertion leaves the caller field set as it found it, so a second safe assertion with the same
    /// set writes its report and does not throw (D-112, F-72).
    /// </summary>
    [Fact]
    public void ASafeAssertionLeavesTheCallerContextAlone()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);
        LogFields fields = RunFields();
        int before = fields.Fields.Count;

        Invariant.Assert(false, "the first invariant", logger, LogContextKind.Run, fields, continueOnFailure: true);
        Invariant.Assert(false, "the second invariant", logger, LogContextKind.Run, fields, continueOnFailure: true);

        Assert.Equal(2, sink.Lines.Count);
        Assert.Equal(before, fields.Fields.Count);
        Assert.False(fields.Has("assertFile"));

        // An ordinary line after a safe failure carries no assertion field.
        logger.Write(LogContextKind.Run, LogLevel.Info, "an ordinary line", fields);
        Assert.DoesNotContain("assertFile", sink.Lines[2], StringComparison.Ordinal);
    }

    /// <summary>
    /// A surrogate that stands without its pair takes an escape, so the line stays valid JSON (F-73). Each row
    /// carries a label, because two rows of raw surrogate text take one test id and one of them never runs.
    /// </summary>
    [Theory]
    [InlineData("the first high surrogate", 0xD800)]
    [InlineData("the last high surrogate", 0xDBFF)]
    [InlineData("the first low surrogate", 0xDC00)]
    [InlineData("the last low surrogate", 0xDFFF)]
    public void AnUnpairedSurrogateKeepsTheLineValid(string label, int codeUnit)
    {
        char lone = (char)codeUnit;
        string[] values =
        [
            "a" + lone + "b",
            lone.ToString(),
            new string(lone, 2),
            lone + "a",
            "a" + lone,
        ];

        foreach (string value in values)
        {
            CollectingSink sink = new();
            JsonlLogger logger = new(sink);

            // The value stands in a field value, in a field name, and in the message of one line.
            LogFields fields = RunFields();
            fields.Add("path", value);
            fields.Add("name" + value, "a value");
            logger.Write(LogContextKind.Run, LogLevel.Info, value, fields);

            string line = Assert.Single(sink.Lines);
            System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(line);

            // A reader must get the value back. The escape form parses and then fails on GetString, so the
            // line carries the replacement character in place of the lone surrogate (F-73).
            string read = document.RootElement.GetProperty("path").GetString()!;
            Assert.DoesNotContain(lone, read);
            Assert.Contains('\uFFFD', read);
            Assert.NotNull(document.RootElement.GetProperty("message").GetString());
        }

        Assert.NotEmpty(label);
    }

    /// <summary>A matched surrogate pair is one character, and it reads back as the same text.</summary>
    [Fact]
    public void AMatchedSurrogatePairReadsBackUnchanged()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);
        LogFields fields = RunFields();
        fields.Add("path", "a😀b");
        logger.Write(LogContextKind.Run, LogLevel.Info, "a message", fields);

        System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(Assert.Single(sink.Lines));
        Assert.Equal("a😀b", document.RootElement.GetProperty("path").GetString());
    }

    /// <summary>
    /// A caller field can never stop the assertion report. The three call-site names are reserved, so an
    /// assertion always writes its report and a safe one always continues (D-112, F-74).
    /// </summary>
    [Theory]
    [InlineData("assertFile")]
    [InlineData("assertLine")]
    [InlineData("assertMember")]
    public void ACallerFieldCannotStopTheAssertionReport(string reserved)
    {
        LogFields fields = RunFields();
        ContextException error = Assert.Throws<ContextException>(() => fields.Add(reserved, "a value"));
        Assert.Contains(reserved, error.Message, StringComparison.Ordinal);

        // The report still lands, and the safe call still returns.
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);
        Invariant.Assert(false, "an invariant", logger, LogContextKind.Run, fields, continueOnFailure: true);

        System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(Assert.Single(sink.Lines));
        Assert.EndsWith("LoggingTests.cs", document.RootElement.GetProperty("assertFile").GetString(), StringComparison.Ordinal);
        Assert.Equal(nameof(this.ACallerFieldCannotStopTheAssertionReport), document.RootElement.GetProperty("assertMember").GetString());
        Assert.True(document.RootElement.GetProperty("assertLine").GetInt64() > 0);
    }

    /// <summary>PR-4 exit test 3. An assertion report carries the seed of the run it failed in.</summary>
    [Fact]
    public void AssertionReportHasSeed()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);

        Assert.Throws<ContextException>(
            () => Invariant.Assert(false, "the tick went backward", logger, LogContextKind.Run, RunFields()));

        string line = Assert.Single(sink.Lines);
        System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(line);
        Assert.Equal(20260908L, document.RootElement.GetProperty("seed").GetInt64());
        Assert.Equal("error", document.RootElement.GetProperty("level").GetString());
        Assert.Equal("the tick went backward", document.RootElement.GetProperty("message").GetString());

        // The report names the call site, which the compiler fills in and a stack walk does not (D-215).
        Assert.EndsWith("LoggingTests.cs", document.RootElement.GetProperty("assertFile").GetString(), StringComparison.Ordinal);
        Assert.True(document.RootElement.GetProperty("assertLine").GetInt64() > 0);
        Assert.Equal(nameof(this.AssertionReportHasSeed), document.RootElement.GetProperty("assertMember").GetString());
    }

    /// <summary>PR-4 exit test 4. A call that the caller marks safe returns after the report (D-112).</summary>
    [Fact]
    public void SafeAssertionContinues()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);

        Invariant.Assert(false, "a safe invariant", logger, LogContextKind.Run, RunFields(), continueOnFailure: true);

        Assert.Single(sink.Lines);
        Assert.Contains("a safe invariant", sink.Lines[0], StringComparison.Ordinal);
    }

    /// <summary>PR-4 exit test 5. A call that the caller did not mark safe throws a ContextException (D-112).</summary>
    [Fact]
    public void UnsafeAssertionThrows()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);

        ContextException error = Assert.Throws<ContextException>(
            () => Invariant.Assert(false, "an unsafe invariant", logger, LogContextKind.Run, RunFields()));

        Assert.Contains("an unsafe invariant", error.Message, StringComparison.Ordinal);
        Assert.Contains("seed", error.Message, StringComparison.Ordinal);
        Assert.Single(sink.Lines);
    }

    /// <summary>A condition that holds writes no line and throws nothing.</summary>
    [Fact]
    public void AnAssertionThatHoldsIsSilent()
    {
        CollectingSink sink = new();
        JsonlLogger logger = new(sink);
        Invariant.Assert(true, "this holds", logger, LogContextKind.Run, RunFields());
        Assert.Empty(sink.Lines);
    }

    /// <summary>PR-4 exit test 6. A catch adds a field and rethrows, and the outer catch reads both fields.</summary>
    [Fact]
    public void RethrowAddsContext()
    {
        Action rethrow = () =>
        {
            try
            {
                ContextException inner = new("the floor generator gave no stairwell");
                inner.AddContext("seed", "20260908");
                throw inner;
            }
            catch (ContextException error)
            {
                error.AddContext("floor", "3");
                throw;
            }
        };

        ContextException caught = Assert.Throws<ContextException>(rethrow);

        Assert.Contains("seed=20260908", caught.Message, StringComparison.Ordinal);
        Assert.Contains("floor=3", caught.Message, StringComparison.Ordinal);
        Assert.Equal(2, caught.Context.Count);
    }

    /// <summary>A repeated context name on one error is an error, so no field hides another (T-2).</summary>
    [Fact]
    public void ARepeatedContextNameIsAnError()
    {
        ContextException error = new("a message");
        error.AddContext("seed", "1");
        Assert.Throws<ContextException>(() => error.AddContext("seed", "2"));
    }

    /// <summary>An error with no context reads as its plain message.</summary>
    [Fact]
    public void AnErrorWithNoContextReadsPlainly()
    {
        ContextException error = new("a plain message");
        Assert.Equal("a plain message", error.Message);
        Assert.Empty(error.Context);
    }

    /// <summary>A context that is not a declared value is an error that names the value (T-2).</summary>
    [Fact]
    public void AnUndeclaredContextIsAnError()
    {
        ContextException error = Assert.Throws<ContextException>(() => JsonlLogger.RequiredFields((LogContextKind)9));
        Assert.Contains("9", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Every declared context has a required field set, so a new value cannot pass the switch in silence.</summary>
    [Fact]
    public void EveryDeclaredContextHasARequiredSet()
    {
        foreach (LogContextKind context in Enum.GetValues<LogContextKind>())
        {
            Assert.NotEmpty(JsonlLogger.RequiredFields(context));
        }
    }

    /// <summary>Every declared level has a name in a line, so a new value cannot reach the file unnamed.</summary>
    [Fact]
    public void EveryDeclaredLevelHasAName()
    {
        foreach (LogLevel level in Enum.GetValues<LogLevel>())
        {
            string line = JsonlLogger.BuildLine(level, "a message", new LogFields());
            System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(line);
            Assert.False(string.IsNullOrEmpty(document.RootElement.GetProperty("level").GetString()));
        }

        ContextException error = Assert.Throws<ContextException>(() => JsonlLogger.BuildLine((LogLevel)9, "a message", new LogFields()));
        Assert.Contains("9", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The line keeps the order in which the caller added the fields, so one seed gives one line.</summary>
    [Fact]
    public void TheLineKeepsTheFieldOrder()
    {
        LogFields first = new();
        first.Add("a", 1L);
        first.Add("b", 2L);

        LogFields second = new();
        second.Add("b", 2L);
        second.Add("a", 1L);

        Assert.NotEqual(JsonlLogger.BuildLine(LogLevel.Info, "m", first), JsonlLogger.BuildLine(LogLevel.Info, "m", second));
        Assert.Equal("{\"level\":\"info\",\"message\":\"m\",\"a\":1,\"b\":2}", JsonlLogger.BuildLine(LogLevel.Info, "m", first));
    }

    private static LogFields RunFields()
    {
        LogFields fields = new();
        fields.Add("seed", 20260908L);
        fields.Add("floor", 3L);
        fields.Add("tick", 1820L);
        fields.Add("subsystem", "combat");
        fields.Add("entities", new List<long> { 41L, 77L });
        return fields;
    }

    private static LogFields HubFields()
    {
        LogFields fields = new();
        fields.Add("saveVersions", "profile=3;run=1");
        fields.Add("screen", "loadout");
        fields.Add("action", "equip");
        fields.Add("paths", "user://profile.json");
        return fields;
    }

    /// <summary>The run field set with one field left out, so each required field can fail on its own.</summary>
    private static LogFields RunFieldsExcept(string omitted)
    {
        LogFields fields = new();
        if (omitted != "seed")
        {
            fields.Add("seed", 20260908L);
        }

        if (omitted != "floor")
        {
            fields.Add("floor", 3L);
        }

        if (omitted != "tick")
        {
            fields.Add("tick", 1820L);
        }

        if (omitted != "subsystem")
        {
            fields.Add("subsystem", "combat");
        }

        if (omitted != "entities")
        {
            fields.Add("entities", new List<long> { 41L, 77L });
        }

        return fields;
    }
}
