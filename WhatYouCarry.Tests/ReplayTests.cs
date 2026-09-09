using System;
using System.Collections.Generic;
using System.Text;
using WhatYouCarry.Core.Logging;
using WhatYouCarry.Core.Replay;
using WhatYouCarry.Core.Simulation;
using Xunit;

namespace WhatYouCarry.Tests;

/// <summary>The run record, the recorder, and the replay (D-97, D-151, D-152, D-163; PR-6 exit tests 1 to 6). The loops run on the flat floor of <see cref="TestWorld"/> (D-236, PR-7).</summary>
public sealed class ReplayTests
{
    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string OtherHash = "fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210";

    /// <summary>A sink that keeps the record in memory, which is the test half of the two callers of D-111.</summary>
    private sealed class MemorySink : IRunRecordSink
    {
        public List<byte[]> Appends { get; } = [];

        public void Append(byte[] bytes)
        {
            this.Appends.Add(bytes);
        }

        public byte[] Record()
        {
            List<byte> all = [];
            foreach (byte[] part in this.Appends)
            {
                all.AddRange(part);
            }

            return all.ToArray();
        }
    }

    private sealed class CollectingSink : ILogSink
    {
        public List<string> Lines { get; } = [];

        public void Write(string line)
        {
            this.Lines.Add(line);
        }
    }

    /// <summary>Records one run of random intents and runs the live loop beside it. Gives the record and the live end hash.</summary>
    private static (byte[] Record, string LiveHash) RecordRun(ulong seed, int frames, Random random)
    {
        MemorySink sink = new();
        RunRecorder recorder = new(sink, RunRecord.NewHeader(Hash, seed));
        SimulationLoop live = new(seed, TestWorld.FlatFloor(), TestWorld.Spawn);
        for (uint tick = 0; tick < frames; tick++)
        {
            Intent intent = SimulationTests.RandomIntent(random, tick);
            recorder.Record(intent);
            live.Step(intent);
        }

        return (sink.Record(), live.Hash().ToString());
    }

    /// <summary>PR-6 exit test 1. The live run and the replay end with one state hash, over one thousand seeds (D-66).</summary>
    [Fact]
    public void ReplayReproducesHash()
    {
        for (int seed = 1; seed <= 1000; seed++)
        {
            Random random = new(seed);
            int frames = random.Next(0, 200);
            (byte[] record, string liveHash) = RecordRun((ulong)seed * 0x9E3779B97F4A7C15UL, frames, random);

            CollectingSink logs = new();
            ReplayResult result = RunReplayer.Replay(record, Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(logs));

            Assert.True(liveHash == result.Loop.Hash().ToString(), $"Seed {seed}: the live hash is {liveHash}, and the replay gives {result.Loop.Hash()}.");
            Assert.True(frames == result.FrameCount, $"Seed {seed}: {frames} frames, and the replay ran {result.FrameCount}.");
            Assert.True(result.TornBytes == 0, $"Seed {seed}: a whole record has no torn tail, and the replay found {result.TornBytes} bytes.");
            Assert.True(logs.Lines.Count == 0, $"Seed {seed}: a whole record writes no log line, and the replay wrote {logs.Lines.Count}.");
        }
    }

    /// <summary>PR-6 exit test 2. A record cut inside its last frame replays to the last complete frame, with one log line (D-152, D-228).</summary>
    [Fact]
    public void TornTailTruncates()
    {
        (byte[] whole, _) = RecordRun(77UL, 5, new Random(5));
        byte[] torn = whole[..(whole.Length - (Intent.FrameSize / 2))];

        CollectingSink logs = new();
        ReplayResult result = RunReplayer.Replay(torn, Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(logs));

        Assert.Equal(4, result.FrameCount);
        Assert.Equal(4U, result.Loop.Tick);
        Assert.Equal(Intent.FrameSize / 2, result.TornBytes);

        string line = Assert.Single(logs.Lines);
        Assert.Contains("\"level\":\"warning\"", line, StringComparison.Ordinal);
        Assert.Contains("\"seed\":77", line, StringComparison.Ordinal);
        Assert.Contains("\"floor\":1", line, StringComparison.Ordinal);
        Assert.Contains("\"tick\":4", line, StringComparison.Ordinal);
        Assert.Contains("\"subsystem\":\"replay\"", line, StringComparison.Ordinal);
        Assert.Contains("\"entities\":[]", line, StringComparison.Ordinal);
        Assert.Contains("\"tornBytes\":8", line, StringComparison.Ordinal);
    }

    /// <summary>A torn tail after the last frame gives the same end state as the whole record.</summary>
    [Fact]
    public void ATornTailKeepsTheCompleteFrames()
    {
        (byte[] whole, string liveHash) = RecordRun(78UL, 10, new Random(6));
        byte[] torn = new byte[whole.Length + 3];
        whole.CopyTo(torn, 0);

        ReplayResult result = RunReplayer.Replay(torn, Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink()));
        Assert.Equal(liveHash, result.Loop.Hash().ToString());
        Assert.Equal(3, result.TornBytes);
    }

    /// <summary>PR-6 exit test 3. One flipped bit in a frame is an error that names the frame (D-162, T-2).</summary>
    [Fact]
    public void FrameChecksumDetectsFlip()
    {
        (byte[] record, _) = RecordRun(79UL, 6, new Random(7));
        int headerLength = record.Length - (6 * Intent.FrameSize);
        record[headerLength + (2 * Intent.FrameSize) + 9] ^= 0x10;

        ContextException error = Assert.Throws<ContextException>(() => RunReplayer.Replay(record, Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink())));
        Assert.Contains("frame=2", error.Message, StringComparison.Ordinal);
        Assert.Contains("storedChecksum", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A frame whose tick is out of order is an error that names the frame, even with a valid checksum.</summary>
    [Fact]
    public void AFrameOutOfOrderNamesTheFrame()
    {
        MemorySink sink = new();
        sink.Append(RunRecord.WriteHeader(RunRecord.NewHeader(Hash, 80UL)));
        sink.Append(new Intent(0U, 0, 0, 0, 0, 0).Encode());
        sink.Append(new Intent(2U, 0, 0, 0, 0, 0).Encode());

        ContextException error = Assert.Throws<ContextException>(() => RunReplayer.Replay(sink.Record(), Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink())));
        Assert.Contains("frame=1", error.Message, StringComparison.Ordinal);
        Assert.Contains("intentTick=2", error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-6 exit test 4. A simulation version that differs is a report that names both versions (D-151).</summary>
    [Fact]
    public void VersionMismatchReports()
    {
        byte[] record = RunRecord.WriteHeader(new RunRecordHeader(RunRecord.FormatVersion, SimulationVersion.Value + 1, Hash, 1UL));

        ContextException error = Assert.Throws<ContextException>(() => RunReplayer.Replay(record, Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink())));
        Assert.Contains($"recordSimulationVersion={SimulationVersion.Value + 1}", error.Message, StringComparison.Ordinal);
        Assert.Contains($"buildSimulationVersion={SimulationVersion.Value}", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A format version that differs fails before any field check, and it names both versions.</summary>
    [Fact]
    public void FormatVersionMismatchReports()
    {
        byte[] record = Encoding.UTF8.GetBytes("{\"formatVersion\":2,\"somethingNew\":true}\n");

        ContextException error = Assert.Throws<ContextException>(() => RunRecord.ReadHeader(record));
        Assert.Contains("recordFormatVersion=2", error.Message, StringComparison.Ordinal);
        Assert.Contains("buildFormatVersion=1", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("somethingNew", error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-6 exit test 5. A content hash that differs is a report that names both hashes (D-151, D-163).</summary>
    [Fact]
    public void ContentMismatchReports()
    {
        (byte[] record, _) = RecordRun(81UL, 3, new Random(8));

        ContextException error = Assert.Throws<ContextException>(() => RunReplayer.Replay(record, OtherHash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink())));
        Assert.Contains($"recordContentHash={Hash}", error.Message, StringComparison.Ordinal);
        Assert.Contains($"buildContentHash={OtherHash}", error.Message, StringComparison.Ordinal);
    }

    /// <summary>PR-6 exit test 6. A header survives a write and a read, field for field, and the body starts after the line break.</summary>
    [Fact]
    public void HeaderRoundTrip()
    {
        RunRecordHeader header = RunRecord.NewHeader(Hash, ulong.MaxValue);
        byte[] line = RunRecord.WriteHeader(header);

        (RunRecordHeader read, int bodyStart) = RunRecord.ReadHeader(line);

        Assert.Equal(header, read);
        Assert.Equal(line.Length, bodyStart);
        Assert.Equal((byte)'\n', line[^1]);
    }

    /// <summary>The header text, byte for byte. A change to the shape is a change to the format version, and the simulation version moves on its own (D-163, G-20).</summary>
    [Fact]
    public void HeaderText()
    {
        string expected = "{\"formatVersion\":1,\"simulationVersion\":" + SimulationVersion.Value + ",\"contentHash\":\"" + Hash + "\",\"seed\":42,\"loadout\":[],\"tree\":[],\"amulet\":null}\n";
        Assert.Equal(expected, Encoding.UTF8.GetString(RunRecord.WriteHeader(RunRecord.NewHeader(Hash, 42UL))));
    }

    /// <summary>A record with no line break holds no header, and that is an error and never an empty run.</summary>
    [Fact]
    public void ARecordWithoutAHeaderLineIsAnError()
    {
        byte[] record = Encoding.UTF8.GetBytes("{\"formatVersion\":1");
        ContextException error = Assert.Throws<ContextException>(() => RunRecord.ReadHeader(record));
        Assert.Contains("no complete header line", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Every required header field is required, one at a time (D-151).</summary>
    [Theory]
    [InlineData("simulationVersion")]
    [InlineData("contentHash")]
    [InlineData("seed")]
    [InlineData("loadout")]
    [InlineData("tree")]
    [InlineData("amulet")]
    public void EveryRequiredHeaderFieldIsRequired(string omitted)
    {
        string whole = Encoding.UTF8.GetString(RunRecord.WriteHeader(RunRecord.NewHeader(Hash, 1UL)));
        int start = whole.IndexOf(",\"" + omitted + "\":", StringComparison.Ordinal);
        int end = whole.IndexOf(",\"", start + 1, StringComparison.Ordinal);
        string cut = end < 0 ? whole[..start] + "}\n" : whole[..start] + whole[end..];

        ContextException error = Assert.Throws<ContextException>(() => RunRecord.ReadHeader(Encoding.UTF8.GetBytes(cut)));
        Assert.Contains(omitted, error.Message, StringComparison.Ordinal);
        Assert.Contains("is absent", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A header field of the wrong shape names the field: a loadout with an item, an amulet with a value, a seed below zero, an unknown name.</summary>
    [Theory]
    [InlineData("\"loadout\":[]", "\"loadout\":[1]", "loadout")]
    [InlineData("\"tree\":[]", "\"tree\":{}", "tree")]
    [InlineData("\"amulet\":null", "\"amulet\":\"none\"", "amulet")]
    [InlineData("\"seed\":42", "\"seed\":-1", "seed")]
    [InlineData("\"seed\":42", "\"seed\":18446744073709551616", "seed")]
    [InlineData("\"seed\":42", "\"seed\":42,\"floor\":1", "floor")]
    [InlineData("\"contentHash\":\"0123", "\"contentHash\":\"0123456789ABCDEF0123", "contentHash")]
    public void AHeaderFieldOfTheWrongShapeNamesTheField(string from, string to, string field)
    {
        string whole = Encoding.UTF8.GetString(RunRecord.WriteHeader(RunRecord.NewHeader(Hash, 42UL)));
        Assert.Contains(from, whole, StringComparison.Ordinal);
        byte[] record = Encoding.UTF8.GetBytes(whole.Replace(from, to, StringComparison.Ordinal));

        ContextException error = Assert.Throws<ContextException>(() => RunRecord.ReadHeader(record));
        Assert.Contains($"'{field}'", error.Message, StringComparison.Ordinal);
    }

    /// <summary>A hash that is not 64 lowercase hexadecimal digits never reaches a record (D-221).</summary>
    [Theory]
    [InlineData("")]
    [InlineData("0123456789abcdef")]
    [InlineData("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF")]
    [InlineData("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcde\"")]
    public void ABadContentHashIsAnError(string hash)
    {
        Assert.Throws<ContextException>(() => RunRecord.NewHeader(hash, 1UL));
        Assert.Throws<ContextException>(() => RunRecord.WriteHeader(new RunRecordHeader(1, 1, hash, 1UL)));
    }

    /// <summary>The recorder writes the header before any frame, and each frame is one append (G-5).</summary>
    [Fact]
    public void TheRecorderWritesTheHeaderFirst()
    {
        MemorySink sink = new();
        RunRecorder recorder = new(sink, RunRecord.NewHeader(Hash, 9UL));
        Assert.Single(sink.Appends);
        Assert.Equal((byte)'{', sink.Appends[0][0]);

        recorder.Record(new Intent(0U, 1, 2, 3, 4, 5));
        Assert.Equal(2, sink.Appends.Count);
        Assert.Equal(Intent.FrameSize, sink.Appends[1].Length);
        Assert.Equal(1U, recorder.NextTick);
    }

    /// <summary>The recorder rejects a frame out of order and names both ticks (G-5, T-2).</summary>
    [Fact]
    public void TheRecorderRejectsAFrameOutOfOrder()
    {
        MemorySink sink = new();
        RunRecorder recorder = new(sink, RunRecord.NewHeader(Hash, 9UL));
        recorder.Record(new Intent(0U, 0, 0, 0, 0, 0));

        ContextException error = Assert.Throws<ContextException>(() => recorder.Record(new Intent(0U, 0, 0, 0, 0, 0)));
        Assert.Contains("expectedTick=1", error.Message, StringComparison.Ordinal);
        Assert.Contains("intentTick=0", error.Message, StringComparison.Ordinal);
        Assert.Equal(2, sink.Appends.Count);
    }

    /// <summary>An observer that keeps the tick of the loop after each replayed frame.</summary>
    private sealed class TickObserver : IReplayObserver
    {
        public List<uint> Ticks { get; } = [];

        public void AfterTick(SimulationLoop loop)
        {
            this.Ticks.Add(loop.Tick);
        }
    }

    /// <summary>The replay calls its observer once after each complete frame, in order, and never for a torn tail or an empty record.</summary>
    [Fact]
    public void TheReplayVisitsEveryCompleteFrame()
    {
        (byte[] whole, _) = RecordRun(82UL, 6, new Random(9));

        TickObserver all = new();
        RunReplayer.Replay(whole, Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink()), all);
        Assert.Equal(new uint[] { 1, 2, 3, 4, 5, 6 }, all.Ticks);

        TickObserver torn = new();
        RunReplayer.Replay(whole[..(whole.Length - (Intent.FrameSize / 2))], Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink()), torn);
        Assert.Equal(new uint[] { 1, 2, 3, 4, 5 }, torn.Ticks);

        TickObserver none = new();
        RunReplayer.Replay(RunRecord.WriteHeader(RunRecord.NewHeader(Hash, 3UL)), Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink()), none);
        Assert.Empty(none.Ticks);
    }

    /// <summary>A record with a header and no frame is a run at tick zero, and not an error.</summary>
    [Fact]
    public void AHeaderAloneReplaysToTickZero()
    {
        byte[] record = RunRecord.WriteHeader(RunRecord.NewHeader(Hash, 3UL));
        ReplayResult result = RunReplayer.Replay(record, Hash, TestWorld.FlatFloor(), TestWorld.Spawn, new JsonlLogger(new CollectingSink()));
        Assert.Equal(0U, result.Loop.Tick);
        Assert.Equal(0, result.FrameCount);
        Assert.Equal(3UL, result.Header.Seed);
    }
}
