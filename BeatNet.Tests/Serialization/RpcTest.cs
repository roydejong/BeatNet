using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Gameplay;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Tests.Serialization;

public class RpcTest
{
    [Test]
    public void TestSimpleRpcWriteAndRead()
    {
        Span<byte> buffer = stackalloc byte[1024];

        // ---

        var rpcWrite = new SetSongStartTimeRpc();
        rpcWrite.SyncTime = 1234;
        rpcWrite.StartTime = 1234567890;

        var writer = new NetWriter(buffer);
        rpcWrite.WriteTo(ref writer);

        // ---

        var rpcRead = new SetSongStartTimeRpc();

        var reader = new NetReader(buffer);
        rpcRead.ReadFrom(ref reader);

        Assert.That(rpcRead.SyncTime, Is.EqualTo(rpcWrite.SyncTime));
        Assert.That(rpcRead.StartTime, Is.EqualTo(rpcWrite.StartTime));
        Assert.That(reader.Position, Is.EqualTo(writer.Position), "Reader position should be at the end of buffer contents");
    }
    
    [Test]
    public void TestComplexRpcWriteAndRead()
    {
        Span<byte> buffer = stackalloc byte[1024];

        // ---

        var rpcWrite = new RecommendBeatmapRpc();
        rpcWrite.SyncTime = 1234;
        rpcWrite.Key = new BeatmapKeyNetSerializable(
            levelID: "abc",
            beatmapCharacteristicSerializedName: "def",
            difficulty: BeatmapDifficulty.ExpertPlus
        );

        var writer = new NetWriter(buffer);
        rpcWrite.WriteTo(ref writer);

        // ---

        var rpcRead = new RecommendBeatmapRpc();

        var reader = new NetReader(buffer);
        rpcRead.ReadFrom(ref reader);

        Assert.That(rpcRead.SyncTime, Is.EqualTo(rpcWrite.SyncTime));
        Assert.That(rpcRead.Key, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(rpcRead.Key!.LevelID, Is.EqualTo(rpcWrite.Key!.LevelID));
            Assert.That(rpcRead.Key!.BeatmapCharacteristicSerializedName, Is.EqualTo(rpcWrite.Key!.BeatmapCharacteristicSerializedName));
            Assert.That(rpcRead.Key!.Difficulty, Is.EqualTo(rpcWrite.Key!.Difficulty));
        });
        Assert.That(reader.Position, Is.EqualTo(writer.Position), "Reader position should be at the end of buffer contents");
    }
}