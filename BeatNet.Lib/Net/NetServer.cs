using System.Net;
using BeatNet.Lib.BeatSaber;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.Net.Events;
using BeatNet.Lib.Net.IO;
using ENet;
using Serilog;

namespace BeatNet.Lib.Net;

public class NetServer
{
    public readonly ushort UdpPort;
    public readonly uint PeerLimit;
    public readonly ManualResetEvent EventWaitHandle;

    private Thread? _serverThread;
    private bool _serverThreadAlive;
    private bool _serverThreadKeepAlive;

    private readonly Dictionary<uint, Peer> _peers;
    private readonly List<uint> _pendingPeers;
    private readonly byte[] _sendBufferMemory;

    public RingBuffer<ConnectEvent> ConnectQueue = null!;
    public RingBuffer<DisconnectEvent> DisconnectQueue = null!;
    public RingBuffer<DisconnectEvent> PendingDisconnectQueue = null!;
    
    public RingBuffer<PacketEvent> ReceiveQueue = null!;
    public RingBuffer<PacketEvent> SendQueue = null!;

    public IPEndPoint BindAddress => new(IPAddress.Any, UdpPort);
    public bool IsRunning => _serverThreadKeepAlive && _serverThreadAlive;

    private ILogger? _logger;

    public NetServer(ushort udpPort, uint peerLimit = NetConsts.MaximumPeers)
    {
        UdpPort = udpPort;
        PeerLimit = peerLimit;
        EventWaitHandle = new ManualResetEvent(true);

        _serverThread = null;
        _serverThreadAlive = false;
        _serverThreadKeepAlive = false;

        _peers = new();
        _pendingPeers = new();
        _sendBufferMemory = GC.AllocateArray<byte>(length: NetConsts.MaximumPacketSize, pinned: true);

        Reset();
    }

    public void SetLogger(ILogger logger) =>
        _logger = logger.ForContext<NetServer>();

    public async Task<bool> Start()
    {
        if (_serverThreadKeepAlive || _serverThreadAlive)
            throw new InvalidOperationException("Cannot start NetServer when it is already running.");

        _logger?.Verbose("Starting net server (Port {UdpPort})...", UdpPort);
        
        NetPayload.EnsureSubBufferPoolInit();
        SerializableRegistry.NoopCallForStaticInit();

        _serverThreadKeepAlive = true;
        _serverThreadAlive = false;

        _serverThread = new(__ServerThread);
        _serverThread.Name = $"NetServer:{UdpPort}";
        _serverThread.Start();

        while (!_serverThreadAlive && _serverThreadKeepAlive)
            // Wait for the server thread to start
            await Task.Delay(10);

        EventWaitHandle.Set();
        return _serverThreadAlive;
    }

    public async Task<bool> Stop()
    {
        if (!_serverThreadKeepAlive && !_serverThreadAlive)
            // Already fully stopped
            return true;

        _logger?.Verbose("Shutting down net server (Port {UdpPort})...", UdpPort);

        _serverThreadKeepAlive = false;

        while (_serverThreadAlive)
            // Wait for the server thread to stop
            await Task.Delay(10);

        EventWaitHandle.Set();
        return true;
    }

    private void Reset()
    {
        _serverThreadAlive = false;
        _serverThreadKeepAlive = false;

        _peers.Clear();
        _pendingPeers.Clear();

        ConnectQueue = new(NetConsts.ConnectionEventBufferSize);
        DisconnectQueue = new(NetConsts.ConnectionEventBufferSize);
        PendingDisconnectQueue = new(NetConsts.ConnectionEventBufferSize);
        
        ReceiveQueue = new(NetConsts.IncomingOutgoingBufferSize);
        SendQueue = new(NetConsts.IncomingOutgoingBufferSize);
    }

    private void __ServerThread()
    {
        _serverThreadAlive = true;

        try
        {
            if (!Library.Initialize())
                throw new Exception("ENet failed to initialize for server thread!");

            Address eNetAddress = new();
            eNetAddress.SetHost("::0"); // bind any address
            eNetAddress.Port = UdpPort;

            using var eNetHost = new Host();
            eNetHost.Create(eNetAddress, (int)PeerLimit, NetConsts.MaximumChannels);

            Event eNetEvent;

            var shutdownCycle = true;
            while (_serverThreadKeepAlive || shutdownCycle)
            {
                // Send: Pending packets
                while (SendQueue.TryDequeue(out var e))
                {
                    if (!_peers.TryGetValue(e.PeerId, out var peer))
                        continue;

                    var payload = e.Payload;
                    var writer = new NetWriter(_sendBufferMemory);
                    writer.WriteSerializable(payload);

                    var packet = default(Packet);
                    var packetFlags = e.Channel == NetChannel.Unreliable
                        ? PacketFlags.Unsequenced
                        : PacketFlags.Reliable;
                    packet.Create(_sendBufferMemory, writer.Position, packetFlags);
                    peer.Send((byte)e.Channel, ref packet);
                }
                
                // Send: Pending disconnect actions
                while (PendingDisconnectQueue.TryDequeue(out var e))
                {
                    if (!_peers.TryGetValue(e.PeerId, out var peer))
                        continue;

                    if (e.Immediate)
                        peer.DisconnectNow(0);
                    else
                        peer.DisconnectLater(0);
                }

                // ENet: Handle any pending events
                while (eNetHost.CheckEvents(out eNetEvent) > 0)
                    HandleEnetEvent(eNetEvent);

                // ENet: Main update / poll for new events
                if (eNetHost.Service(NetConsts.PollTime, out eNetEvent) > 0)
                    HandleEnetEvent(eNetEvent);

                // Ensure we do a complete cycle during shutdown before stopping the thread
                if (!_serverThreadKeepAlive)
                    shutdownCycle = false;
                
                // Note: We don't need to sleep here, enet polling will block for ~1ms if there are no events
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Net server thread failed");
        }
        finally
        {
            Library.Deinitialize();
            Reset();
        }
    }

    private void HandleEnetEvent(Event e)
    {
        if (e.Type == EventType.None)
            return;
        
        if (!e.Peer.IsSet)
            return;

        switch (e.Type)
        {
            case EventType.Connect:
            {
                if (_peers.ContainsKey(e.Peer.ID))
                    return;

                _peers[e.Peer.ID] = e.Peer;
                if (!_pendingPeers.Contains(e.Peer.ID))
                    _pendingPeers.Add(e.Peer.ID);
                
                EventWaitHandle.Set();
                break;
            }
            case EventType.Disconnect:
            case EventType.Timeout:
            {
                if (!_peers.ContainsKey(e.Peer.ID))
                    return;

                _pendingPeers.Remove(e.Peer.ID);
                _peers.Remove(e.Peer.ID);

                DisconnectQueue.Enqueue(new DisconnectEvent(
                    peerId: e.Peer.ID,
                    immediate: true,
                    gameReason: (e.Type == EventType.Timeout
                        ? DisconnectedReason.Timeout
                        : DisconnectedReason.ClientConnectionClosed)
                ));
                
                EventWaitHandle.Set();
                return;
            }
            case EventType.Receive:
            {
                if (!_peers.ContainsKey(e.Peer.ID))
                    return;
                
                if (!e.Packet.IsSet)
                    return;

                if (e.Packet.Length > NetConsts.MaximumPacketSize)
                {
                    e.Packet.Dispose();
                    return;
                }
                
                unsafe
                {
                    try
                    {
                        var buffer = new Span<byte>(e.Packet.Data.ToPointer(), e.Packet.Length);
                        var reader = new NetReader(buffer);

                        if (_pendingPeers.Contains(e.Peer.ID))
                        {
                            // This is a pending peer, they should be sending their connection request
                            try
                            {
                                var connRequest = reader.ReadSerializable<ConnectionRequest>();
                                if (!connRequest.IsValid)
                                    throw new InvalidDataException("Invalid connection request");
                                
                                _pendingPeers.Remove(e.Peer.ID);
                                
                                ConnectQueue.Enqueue(new ConnectEvent(
                                    peer: e.Peer,
                                    connectionRequest: connRequest
                                ));
                                EventWaitHandle.Set();
                            }
                            catch (Exception)
                            {
                                _logger?.Warning("({Port}) Peer {PeerId} sent invalid connection request",
                                    UdpPort, e.Peer.ID);
                                e.Peer.DisconnectNow(0);
                                return;
                            }

                            return;
                        }
                        
                        // This is an established peer, they should be sending a payload
                        var payload = reader.ReadSerializable<NetPayload>();
                        
                        ReceiveQueue.Enqueue(new PacketEvent(
                            peerId: e.Peer.ID,
                            channel: (NetChannel)e.ChannelID,
                            payload: payload
                        ));
                        EventWaitHandle.Set();
                    }
                    catch (Exception ex)
                    {
                        _logger?.Error(ex, "Packet read error");
                        return;
                    }
                    finally
                    {
                        e.Packet.Dispose();
                    }
                }

                return;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(e.Type), "ENet event type not implemented");
            }
        }
    }
}