using System.Diagnostics;
using BeatNet.GameServer.GameModes;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.Packet;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.Events;
using BeatNet.Lib.Net.Interfaces;
using Serilog;

namespace BeatNet.GameServer.Lobby;

public class LobbyHost
{
    public readonly ushort PortNumber;

    private NetServer _server;
    private Thread _thread;
    private bool _threadRunning;
    private ILogger? _logger;

    private List<byte> _availableConnectionIds;
    private Dictionary<ushort, LobbyPlayer> _players;
    private Dictionary<uint, LobbyPlayer> _playersByPeer;

    public List<LobbyPlayer> PlayerList => _players.Values.ToList();

    public GameMode GameMode { get; set; } = null!;
    public int MaxPlayerCount { get; set; }
    public string? Password { get; set; }

    public bool IsRunning => _server.IsRunning;
    public int PlayerCount => _players.Count;
    public bool IsEmpty => PlayerCount == 0;
    public bool IsFull => PlayerCount >= MaxPlayerCount;
    public bool IsPublic => string.IsNullOrEmpty(Password);

    public LobbyHost(ushort portNumber, int maxPlayerCount = DefaultMaxPlayerCount, GameMode? gameMode = null,
        string? password = null)
    {
        PortNumber = portNumber;

        _server = new NetServer(portNumber);
        _thread = new Thread(__LobbyThread);
        _threadRunning = false;
        _logger = null;

        _availableConnectionIds = new();
        _players = new();
        _playersByPeer = new();

        SetMaxPlayerCount(maxPlayerCount);
        SetGameMode(gameMode ?? new QuickPlayGameMode(this));
        SetPassword(password);
    }

    public void SetLogger(ILogger logger)
    {
        _logger = logger
            .ForContext<LobbyHost>()
            .ForContext("Port", PortNumber);
        _server.SetLogger(_logger);
    }

    public void SetMaxPlayerCount(int maxPlayerCount)
    {
        if (!IsEmpty)
            throw new InvalidOperationException("Cannot change max player count while players are in the lobby.");

        MaxPlayerCount = maxPlayerCount;

        _availableConnectionIds.Clear();
        for (byte i = 0; i < maxPlayerCount; i++)
            _availableConnectionIds.Add(i);

        // TODO Server browser update
    }

    public void SetGameMode(GameMode gameMode)
    {
        if (!IsEmpty)
            throw new InvalidOperationException("Cannot change game mode while players are in the lobby.");

        GameMode = gameMode;
        GameMode.Reset();

        // TODO Server browser update
    }

    public void SetPassword(string? password)
    {
        Password = password;

        // TODO Server browser update
    }

    public async Task<bool> Start()
    {
        // Wait for previous lobby thread to stop (in case of restart)
        while (_threadRunning)
            await Task.Delay(10);

        // Start net server
        var serverStarted = await _server.Start();
        if (!serverStarted)
            return false;

        // Initial state
        GameMode.Reset();

        // TODO Server browser update (fire and forget async)

        // Start thread
        _thread.Name = $"LobbyHost:{PortNumber}";
        _thread.Start();

        // Done!
        _logger?.Information("Started lobby server (Port {Port}, {GameMode}, {MaxPlayerCount} players)",
            PortNumber, GameMode.GetType().Name, MaxPlayerCount);
        return true;
    }

    public async Task Stop(bool immediate = false)
    {
        if (!_server.IsRunning)
            // Already stopped
            return;

        if (!immediate)
        {
            // Kick all players with "server shutting down" message
            KickAllPlayers(DisconnectedReason.ServerTerminated);
        }

        // TODO Server browser update (fire and forget async)

        // Stop net server
        await _server.Stop();

        // Wait for lobby thread to stop
        while (_threadRunning)
            await Task.Delay(10);

        // Reset state
        _players.Clear();
        _playersByPeer.Clear();
    }

    private void __LobbyThread()
    {
        _threadRunning = true;

        try
        {
            var swLastTick = new Stopwatch();
            swLastTick.Start();

            while (IsRunning)
            {
                // Receive: connections
                if (_server.ConnectQueue.TryDequeue(out var connectEvent))
                {
                    if (!connectEvent.Peer.IsSet)
                        continue;

                    var logEndPoint = $"[{connectEvent.Peer.IP}]:{connectEvent.Peer.Port}";

                    if (TryGetNextConnectionId(out var newConnectionId))
                    {
                        var player = new LobbyPlayer(this, connectEvent.Peer.ID, newConnectionId);
                        player.SetConnectionRequest(connectEvent.ConnectionRequest);
                        
                        _players[newConnectionId] = player;
                        _playersByPeer[connectEvent.Peer.ID] = player;

                        _logger?.Debug("({Port}) Player #{Slot} connected ({UserId}, {UserName}, {EndPoint})",
                            PortNumber, newConnectionId, player.UserId, player.UserName, logEndPoint);
                    }
                    else
                    {
                        KickPeer(connectEvent.Peer.ID, immediate: false, DisconnectedReason.ServerAtCapacity);

                        _logger?.Warning("({Port}) Lobby full, rejecting new connection: {EndPoint}",
                            PortNumber, logEndPoint);
                    }
                }

                // Receive: disconnections
                if (_server.DisconnectQueue.TryDequeue(out var disconnectEvent))
                {
                    if (_playersByPeer.TryGetValue(disconnectEvent.PeerId, out var player))
                    {
                        player.Disconnected = true;
                        
                        _players.Remove(player.ConnectionId);
                        _playersByPeer.Remove(disconnectEvent.PeerId);

                        _availableConnectionIds.Add(player.ConnectionId);

                        var logEndPoint = $"[{connectEvent.Peer.IP}]:{connectEvent.Peer.Port}";
                        _logger?.Debug("({Port}) Player #{Slot} disconnected ({UserId}, {UserName}, {EndPoint})",
                            PortNumber, player.ConnectionId, player.UserId, player.UserName, logEndPoint);
                    }
                }
                
                // Receive: messages
                if (_server.ReceiveQueue.TryDequeue(out var packetEvent))
                    if (_playersByPeer.TryGetValue(packetEvent.PeerId, out var player) && !player.Disconnected)
                        HandleReceive(player, packetEvent.Payload);
                
                // TODO Ping check players / timeout
                
                // Game mode tick
                if (swLastTick.ElapsedMilliseconds > 1000)
                {
                    GameMode.Tick();
                    swLastTick.Restart();
                }

                // TODO Periodic server browser update

                // Wait for network events up to 100ms
                _server.EventWaitHandle.WaitOne(100);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Lobby thread failed");
        }
        finally
        {
            _threadRunning = false;
        }
    }

    private bool TryGetNextConnectionId(out byte connectionId)
    {
        if (_availableConnectionIds.Count == 0)
        {
            connectionId = 0;
            return false;
        }

        // Get lowest available slot
        connectionId = _availableConnectionIds.Min();
        return true;
    }

    private void KickPlayer(LobbyPlayer player, DisconnectedReason reason)
    {
        // Mark as disconnected (will not accept new messages)
        player.Disconnected = true;

        // Notify and disconnect peer
        KickPeer(player.PeerId, immediate: false, reason);

        // Notify all other players in the lobby
        SendAll(new PlayerDisconnectedPacket(reason), senderId: player.ConnectionId);
    }

    private void KickPeer(uint peerId, bool immediate, DisconnectedReason reason = DisconnectedReason.Unknown)
    {
        _logger?.Debug("({Port}) Kicking peer {PeerId} ({Reason}, {Mode})",
            PortNumber, peerId, reason, immediate ? "Immediate" : "Graceful");

        if (!immediate)
        {
            // Send kick request / disconnect reason to peer (will only arrive if not immediate)
            Send(peerId, new KickPlayerPacket(reason));
        }

        _server.PendingDisconnectQueue.Enqueue(new DisconnectEvent(
            peerId: peerId,
            immediate: immediate,
            gameReason: reason
        ));
    }

    private void KickAllPlayers(DisconnectedReason reason)
    {
        foreach (var player in _players.Values)
            KickPlayer(player, reason);
    }

    private void SendAll(INetSerializable message, NetChannel channel = NetChannel.Reliable, byte senderId = 0)
    {
        foreach (var player in _players.Values.Where(player => !player.Disconnected))
            SendDirect(player, message, channel);
    }

    private void SendDirect(LobbyPlayer player, INetSerializable message, NetChannel channel = NetChannel.Reliable,
        byte senderId = 0)
        => Send(player.PeerId, message, channel, senderId);

    private void Send(uint peerId, INetSerializable message, NetChannel channel = NetChannel.Reliable,
        byte senderId = 0, byte receiverId = 0)
    {
        var payload = new NetPayload(
            message: message,
            senderId: senderId,
            receiverId: receiverId,
            packetOptions: PacketOption.None
        );
        _server.SendQueue.Enqueue(new PacketEvent(
            peerId: peerId,
            channel: channel,
            payload: payload
        ));
    }

    private void HandleReceive(LobbyPlayer player, NetPayload payload)
    {
        if (payload.Messages == null)
            return;
        
        foreach (var message in payload.Messages)
        {
            _logger?.Information("TEMP: Received message from player {PlayerId} ({MessageType})",
                player.ConnectionId, message.GetType().Name);
        }
    }

    public const int DefaultMaxPlayerCount = 5;
}