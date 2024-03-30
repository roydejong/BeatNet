using BeatNet.Lib;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.BeatSaber.Generated.Packet;
using BeatNet.Lib.BeatSaber.Util;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Tests.Serialization;

public class PacketTest
{
    [SetUp]
    public void Setup()
    {
        SerializableRegistry.NoopCallForStaticInit();
        NetPayload.EnsureSubBufferPoolInit();
    }

    [Test]
    public void TestIdentityReadWrite()
    {
        // -------------------------------------------------------------------------------------------------------------
        // Setup

        var hash = new PlayerStateHash(BitMask128.MinValue);
        hash.Add("some_value");
        hash.Add("another_value");
        
        var fakeAvatarSystemHash = "fake_avatar".MurmurHash2();
        
        var supportedAvatars = BitMask128.MinValue;
        supportedAvatars.AddEntryHash(fakeAvatarSystemHash);
        
        var fakeAvatar = new MultiplayerAvatarData(fakeAvatarSystemHash, [1, 2, 3, 4, 5, 6, 7, 8]);

        var identity = new PlayerIdentityPacket(hash, 
            new MultiplayerAvatarsData([fakeAvatar],supportedAvatars));
        
        identity.Random.SetData([32, 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1]);
        identity.PublicEncryptionKey.SetData([5, 4, 3, 2, 1]);
        
        // -------------------------------------------------------------------------------------------------------------
        // Write
        
        Span<byte> buffer = stackalloc byte[1024];
        
        var writer = new NetWriter(buffer);
        identity.WriteTo(ref writer);
        
        // -------------------------------------------------------------------------------------------------------------
        // Read
        
        var reader = new NetReader(buffer);
        var readIdentity = reader.ReadSerializable<PlayerIdentityPacket>();
        
        // PlayerState
        Assert.Multiple(() =>
        {
            Assert.That(readIdentity.PlayerState.BloomFilter.D0, Is.EqualTo(hash.BloomFilter.D0));
            Assert.That(readIdentity.PlayerState.BloomFilter.D1, Is.EqualTo(hash.BloomFilter.D1));
            Assert.That(readIdentity.PlayerState.Contains("some_value"), Is.True);
            Assert.That(readIdentity.PlayerState.Contains("another_value"), Is.True);
        });
        // MultiplayerAvatarsData
        Assert.Multiple(() =>
        {
            Assert.That(readIdentity.PlayerAvatar.MultiplayerAvatarsDataValue, Has.Count.EqualTo(1));
            Assert.That(readIdentity.PlayerAvatar.MultiplayerAvatarsDataValue[0].AvatarTypeIdentifierHash, Is.EqualTo(fakeAvatarSystemHash));
            Assert.That(readIdentity.PlayerAvatar.MultiplayerAvatarsDataValue[0].Data, Is.EquivalentTo(fakeAvatar.Data));
            Assert.That(readIdentity.PlayerAvatar.SupportedAvatarTypeIdHashesBloomFilter.D0, Is.EqualTo(supportedAvatars.D0));
            Assert.That(readIdentity.PlayerAvatar.SupportedAvatarTypeIdHashesBloomFilter.D1, Is.EqualTo(supportedAvatars.D1));
        });
        // Random / Key
        Assert.Multiple(() =>
        {
            Assert.That(readIdentity.Random.CopyData(), Is.EquivalentTo(identity.Random.CopyData()));
            Assert.That(readIdentity.PublicEncryptionKey.CopyData(), Is.EquivalentTo(identity.PublicEncryptionKey.CopyData()));
        });
    }
}