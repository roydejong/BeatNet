using System.Net;
using BeatNet.GameServer.GameModes;
using BeatNet.Lib.BeatSaber;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.BeatSaber.Generated.Packet;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;
using ENet;

namespace BeatNet.Tests.LobbyHost;

public class NetworkTest
{
    [SetUp]
    public void SetUp()
    {
        Library.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        Library.Deinitialize();
    }

    [Test]
    public void TestClientHostCommunication()
    {
        // Init
        var lobbyHost = new GameServer.Lobby.LobbyHost(12345, IPAddress.Any, 5, QuickPlayGameMode.Id);
        Assert.That(lobbyHost.PortNumber, Is.EqualTo(12345));

        // Start
        var started = lobbyHost.Start().Result;
        Assert.Multiple(() =>
        {
            Assert.That(started, Is.True);
            Assert.That(lobbyHost.IsRunning, Is.True);
        });

        // Client connect + send
        var client = new Host();
        client.Create();

        try
        {
            Span<byte> buffer = stackalloc byte[1024];
            var writer = new NetWriter(buffer);

            var address = new Address();
            address.SetHost("127.0.0.1");
            address.Port = 12345;

            client.Connect(address, NetConsts.MaximumChannels);

            // Poll for connected event
            var connectPoll = client.Service(1000, out var eConnect);
            Assert.Multiple(() =>
            {
                Assert.That(connectPoll, Is.GreaterThanOrEqualTo(1));
                Assert.That(eConnect.Type, Is.EqualTo(EventType.Connect));
            });
            var serverPeer = eConnect.Peer;

            // Send a connection request
            var connRequest = new ConnectionRequest()
            {
                UserId = "TestId",
                UserName = "TestName",
                IsConnectionOwner = false,
                PlayerSessionId = "psess-1234567890"
            };
            writer.WriteSerializable(connRequest);

            var packet = new Packet();
            packet.Create(buffer[..writer.Position].ToArray(), PacketFlags.Reliable);
            serverPeer.Send((byte)NetChannel.ReliableOrdered, ref packet);
            client.Service(100, out _);

            // Check player list
            Assert.That(lobbyHost.PlayerList.Count, Is.EqualTo(1));
            var player = lobbyHost.PlayerList[0];
            Assert.Multiple(() =>
            {
                Assert.That(player.UserId, Is.EqualTo(connRequest.UserId));
                Assert.That(player.UserName, Is.EqualTo(connRequest.UserName));
                Assert.That(player.PlayerSessionId, Is.EqualTo(connRequest.PlayerSessionId));
            });

            // Send identity
            var stateHash = new PlayerStateHash(BitMask128.MaxValue);
            var identity = new PlayerIdentityPacket
            (
                playerState: stateHash,
                playerAvatar: new(new(), BitMask128.MaxValue)
            );
            
            writer.Reset();
            writer.WriteSerializable(new NetPayload(identity));
            
            packet.Create(buffer[..writer.Position].ToArray(), PacketFlags.Reliable);
            serverPeer.Send((byte)NetChannel.ReliableOrdered, ref packet);
            client.Service(100, out _);
            client.Service(100, out _);
            
            // Check identity set
            Assert.Multiple(() =>
            {
                Assert.That(player.State, Is.Not.Null);
                Assert.That(player.State!.BloomFilter.D0, Is.EqualTo(identity.PlayerState.BloomFilter.D0));
                Assert.That(player.State!.BloomFilter.D1, Is.EqualTo(identity.PlayerState.BloomFilter.D1));
            });

            // Disconnect
            serverPeer.DisconnectNow(0);
            client.Service(100, out _);

            // Check player list
            Assert.That(lobbyHost.PlayerList.Count, Is.EqualTo(0));
        }
        finally
        {
            client.Dispose();
            client = null;
            
            // Stop
            var stopTask = lobbyHost.Stop();
            stopTask.Wait();
        }

        Assert.That(lobbyHost.IsRunning, Is.False);
    }
}