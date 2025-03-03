using BeatNet.Lib;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;
using BeatNet.Lib.MultiplayerCore;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Tests.Serialization;

public class PayloadTest
{
    [SetUp]
    public void Setup()
    {
        SerializableRegistry.NoopCallForStaticInit();
        NetPayload.EnsureSubBufferPoolInit();
    }
    
    [Test]
    public void TestMergedMessageWriteRead()
    {
        Span<byte> buffer = stackalloc byte[1024];

        // ---
        
        var writePayload = new NetPayload();
        writePayload.SenderId = 5;
        writePayload.ReceiverId = 127;
        writePayload.PacketOptions = PacketOption.OnlyFirstDegreeConnections | PacketOption.Encrypted;

        var msgOne = new RequestKickPlayerRpc("bla123");
        writePayload.Messages.Add(msgOne);

        var msgTwo = new GenericMpCorePacket();
        msgTwo.PacketNameValue = "FakePacket";
        msgTwo.Payload = new byte[] { 0x01, 0x02, 0x03 };
        writePayload.Messages.Add(msgTwo);
        
        var writer = new NetWriter(buffer);
        writePayload.WriteTo(ref writer);

        // ---
        
        var reader = new NetReader(buffer);
        
        var readPayload = new NetPayload();
        readPayload.ReadFrom(ref reader);
        
        // ---
        
        // Packet header
        Assert.Multiple(() =>
        {
            Assert.That(readPayload.SenderId, Is.EqualTo(writePayload.SenderId));
            Assert.That(readPayload.ReceiverId, Is.EqualTo(writePayload.ReceiverId));
            Assert.That(readPayload.PacketOptions, Is.EqualTo(writePayload.PacketOptions));
        });
        
        // Packet contents
        Assert.Multiple(() =>
        {
            Assert.That(readPayload.Messages, Has.Count.EqualTo(2));
            Assert.That(readPayload.Messages[0], Is.InstanceOf<RequestKickPlayerRpc>());
            Assert.That(readPayload.Messages[1], Is.InstanceOf<GenericMpCorePacket>());
        });

        // Packet message 1 - Menu RPC
        var message1 = (RequestKickPlayerRpc)readPayload.Messages[0];
        Assert.That(message1.KickedPlayerId, Is.EqualTo(msgOne.KickedPlayerId));
        
        // Packet message 2 - MPC Generic
        var message2 = (GenericMpCorePacket)readPayload.Messages[1];
        Assert.Multiple(() =>
        {
            Assert.That(message2.PacketNameValue, Is.EqualTo(msgTwo.PacketNameValue));
            Assert.That(message2.Payload, Is.EqualTo(msgTwo.Payload));
        });
    }
}