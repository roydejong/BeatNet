using BeatNet.Lib.BeatSaber.Generated.Rpc.Gameplay;
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
        rpcWrite.StartTime = 1234567890;
        
        var writer = new NetWriter(buffer);
        rpcWrite.WriteTo(ref writer);
        
        Assert.That(writer.Position, Is.Not.EqualTo(0),
            "Writer position should have advanced after RPC WriteTo");
        
        // ---

        var rpcRead = new SetSongStartTimeRpc();
        
        var reader = new NetReader(buffer);
        rpcRead.ReadFrom(ref reader);
        
        Assert.That(rpcRead.StartTime, Is.EqualTo(rpcWrite.StartTime));
        Assert.That(reader.Position, Is.EqualTo(writer.Position),
            "Reader position should have advanced to the entire buffer content");
    }
}