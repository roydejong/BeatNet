using System.Diagnostics;
using BeatNet.GameServer.GameModes;
using BeatNet.GameServer.Util;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.MultiplayerSession;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.BeatSaber.Generated.Packet;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;
using BeatNet.Lib.BeatSaber.Util;
using BeatNet.Lib.MultiplayerCore;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.Events;
using BeatNet.Lib.Net.Interfaces;
using Serilog;

namespace BeatNet.GameServer.Lobby;

public class LobbyHost
{
    public readonly ushort PortNumber;
    public readonly string ServerUserId;

    private readonly NetServer _server;
    private readonly Thread _thread;
    private bool _threadRunning;
    private ILogger? _logger;

    private readonly Queue<byte> _availableConnectionIds;
    private readonly Queue<int> _availableSortIndices;
    private readonly Dictionary<ushort, LobbyPlayer> _players;
    private readonly Dictionary<uint, LobbyPlayer> _playersByPeer;
    private bool _sortIndexUpdateNeeded;

    public IReadOnlyList<LobbyPlayer> PlayerList => _players.Values.ToList();
    public IReadOnlyList<LobbyPlayer> ConnectedPlayers => _players.Values.Where(p => !p.Disconnected).ToList();

    public SyncTimeProvider TimeProvider { get; private set; }
    public GameMode GameMode { get; private set; } = null!;
    public int MaxPlayerCount { get; private set; }
    public string? Password { get; private set; }

    public string ServerName => $"BeatNet {PortNumber}";
    public string GameModeType => GameMode.GetType().Name;
    public string GameModeName => GameMode.GetName();
    public bool IsRunning => _server.IsRunning;
    public int PlayerCount => _players.Count;
    public bool IsEmpty => PlayerCount == 0;
    public bool IsFull => PlayerCount >= MaxPlayerCount;
    public bool IsPublic => string.IsNullOrEmpty(Password);
    public long SyncTime => TimeProvider.GetRunTimeMs();

    private long _lastPingTime = 0;

    public LobbyHost(ushort portNumber, int maxPlayerCount = DefaultMaxPlayerCount, GameMode? gameMode = null,
        string? password = null)
    {
        PortNumber = portNumber;
        ServerUserId = "beatnet:" + RandomId.Generate(8);

        _server = new NetServer(portNumber);
        _thread = new Thread(__LobbyThread);
        _threadRunning = false;
        _logger = null;

        _availableConnectionIds = new(maxPlayerCount);
        _availableSortIndices = new(maxPlayerCount);
        _players = new(maxPlayerCount);
        _playersByPeer = new(maxPlayerCount);
        _sortIndexUpdateNeeded = false;

        TimeProvider = new SyncTimeProvider();
        
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
        for (byte i = 1; i <= maxPlayerCount; i++)
            _availableConnectionIds.Enqueue(i);
        
        _availableSortIndices.Clear();
        for (int i = 0; i < maxPlayerCount; i++)
            _availableSortIndices.Enqueue(i);

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
        TimeProvider.Reset();
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
            await Task.Delay(50); // give some time to ensure NetServer completes sends
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

    #region Lobby Update

    private const int GameModeTickMs = 1000;
    private const int PingIntervalMs = 2000;

    private void __LobbyThread()
    {
        _threadRunning = true;

        try
        {
            var swLastTick = new Stopwatch();
            swLastTick.Start();

            while (IsRunning)
            {
                _server.EventWaitHandle.Reset();
                
                _UpdateConnectQueue();
                _UpdateDisconnectQueue();
                _UpdateReceiveQueue();
                _UpdatePingChecks();
                _UpdateSortIndexes();
                
                // Game mode tick
                if (swLastTick.ElapsedMilliseconds >= GameModeTickMs)
                {
                    GameMode.Tick();
                    swLastTick.Restart();
                }

                _UpdateServerBrowser();

                // Wait for new network events, up to 100ms
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

    private void _UpdateConnectQueue()
    {
        // Receive: incoming connections
        while (_server.ConnectQueue.TryDequeue(out var connectEvent))
        {
            if (!connectEvent.Peer.IsSet)
                continue;

            var logEndPoint = $"[{connectEvent.Peer.IP}]:{connectEvent.Peer.Port}";

            if (!TryGetNextConnectionId(out var newConnectionId))
            {
                _logger?.Warning("({Port}) Lobby full, rejecting new connection: {EndPoint}",
                    PortNumber, logEndPoint);
                KickPeer(connectEvent.Peer.ID, immediate: false, DisconnectedReason.ServerAtCapacity);
                return;
            }

            var player = new LobbyPlayer(this, connectEvent.Peer.ID, newConnectionId);
            player.SetConnectionRequest(connectEvent.ConnectionRequest);

            if (string.IsNullOrEmpty(player.UserId))
            {
                _logger?.Warning("({Port}) Rejecting connection with empty / invalid User ID: {EndPoint}",
                    PortNumber, logEndPoint);
                KickPeer(connectEvent.Peer.ID, immediate: false, DisconnectedReason.Kicked);
                return;
            }
            
            if (_players.Values.Any(p => p.UserId == player.UserId))
            {
                _logger?.Warning("({Port}) Rejecting connection with duplicate User ID: {EndPoint}",
                    PortNumber, logEndPoint);
                KickPeer(connectEvent.Peer.ID, immediate: false, DisconnectedReason.Kicked);
                return;
            }
                    
            _players[newConnectionId] = player;
            _playersByPeer[connectEvent.Peer.ID] = player;

            _logger?.Information("({Port}) Player #{Slot} connected ({UserId}, {UserName}, {EndPoint})",
                PortNumber, newConnectionId, player.UserId, player.UserName, logEndPoint);
                    
            HandlePlayerConnect(player);
        }
    }

    private void _UpdateDisconnectQueue()
    {
        // Receive: disconnections
        while (_server.DisconnectQueue.TryDequeue(out var disconnectEvent))
        {
            if (!_playersByPeer.TryGetValue(disconnectEvent.PeerId, out var player))
                continue;

            if (!player.Disconnected)
            {
                // Player was not already kicked, so we need to notify other clients
                player.Disconnected = true;
                SendToAllFrom(new PlayerDisconnectedPacket(DisconnectedReason.ClientConnectionClosed), player.ConnectionId);
            }

            _players.Remove(player.ConnectionId);
            _playersByPeer.Remove(disconnectEvent.PeerId);

            _availableConnectionIds.Enqueue(player.ConnectionId);
            if (player.SortIndex != null)
                _availableSortIndices.Enqueue(player.SortIndex.Value);

            _logger?.Information("({Port}) Player #{Slot} disconnected ({UserId}, {UserName})",
                PortNumber, player.ConnectionId, player.UserId, player.UserName);
                        
            HandlePlayerDisconnect(player);
        }
    }

    private void _UpdateReceiveQueue()
    {
        // Receive: packets
        while (_server.ReceiveQueue.TryDequeue(out var packetEvent))
        {
            if (!_playersByPeer.TryGetValue(packetEvent.PeerId, out var player) || player.Disconnected)
                continue;
            
            HandleReceive(player, packetEvent.Payload);
        }
    }

    private void _UpdatePingChecks()
    {
        if (PlayerCount == 0)
            return;
        
        var timeSinceLastPing = SyncTime - _lastPingTime;
        if (timeSinceLastPing < PingIntervalMs)
            return;
        
        _lastPingTime = SyncTime;
        SendToAll(new PingPacket(_lastPingTime));
        
        // TODO: Disconnect players that are not responding to pings / have very high latency
    }

    private void _UpdateSortIndexes()
    {
        if (!_sortIndexUpdateNeeded)
            return;
        
        // Find valid players without a sort index, assign next available index
        // Players must have valid ping and must have sent identity with valid state
        
        var playersWithoutSort = _players.Values
            .Where(p => p is
            {
                Disconnected: false,
                HasValidLatency: true,
                SortIndex: null,
                StatePlayer: true
            })
            .ToList();

        foreach (var player in playersWithoutSort)
        {
            if (!TryGetNextSortIndex(out var sortIndex))
                continue;
            
            player.SortIndex = sortIndex;
            SendToAll(player.GetPlayerSortOrderPacket());
            GameMode.OnPlayerSpawn(player);
        }
        
        _sortIndexUpdateNeeded = false;
    }

    private void _UpdateServerBrowser()
    {
        // TODO Periodic server browser update
    }

    #endregion

    #region Player Management
    
    private bool TryGetNextConnectionId(out byte connectionId)
    {
        if (_availableConnectionIds.Count == 0)
        {
            connectionId = 0;
            return false;
        }

        connectionId = _availableConnectionIds.Dequeue();
        return true;
    }
    
    private bool TryGetNextSortIndex(out int sortIndex)
    {
        if (_availableSortIndices.Count == 0)
        {
            sortIndex = -1;
            return false;
        }

        sortIndex = _availableSortIndices.Dequeue();
        return true;
    }

    private void KickPlayer(LobbyPlayer player, DisconnectedReason reason)
    {
        // Mark as disconnected (will not accept new messages)
        player.Disconnected = true;

        // Notify and disconnect peer
        KickPeer(player.PeerId, immediate: false, reason);

        // Notify all other players in the lobby
        SendToAllFrom(new PlayerDisconnectedPacket(reason), player.ConnectionId);
    }

    private void KickPeer(uint peerId, bool immediate, DisconnectedReason reason = DisconnectedReason.Unknown)
    {
        _logger?.Information("({Port}) Kicking peer {PeerId} ({Reason}, {Mode})",
            PortNumber, peerId, reason, immediate ? "Immediate" : "Graceful");

        if (!immediate)
        {
            // Send kick request / disconnect reason to peer (will only arrive if not immediate)
            _Send(peerId, new KickPlayerPacket(reason));
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
    
    #endregion

    #region Send APIs

    public void SendTo(INetSerializable message, LobbyPlayer to, NetChannel channel = NetChannel.Reliable)
        => _Send(to.PeerId, message, channel);
    
    public void SendToFrom(INetSerializable message, LobbyPlayer to, byte from, NetChannel channel = NetChannel.Reliable)
        => _Send(to.PeerId, message, channel, from);

    public void SendToAll(INetSerializable message, NetChannel channel = NetChannel.Reliable)
    {
        foreach (var player in ConnectedPlayers)
            SendTo(message, player, channel);
    }

    public void SendToAllFrom(INetSerializable message, byte from, NetChannel channel = NetChannel.Reliable)
    {
        foreach (var to in ConnectedPlayers.Where(player => player.ConnectionId != from))
            SendToFrom(message, to, from, channel);
    }
    
    public void SendToAllExcluding(INetSerializable message, byte? excluding = null, NetChannel channel = NetChannel.Reliable)
    {
        foreach (var to in ConnectedPlayers.Where(player => player.ConnectionId != excluding))
            SendTo(message, to, channel);
    }

    private void _Send(uint peerId, INetSerializable message, NetChannel channel = NetChannel.Reliable,
        byte senderId = 0)
    {
        if (message is BaseRpc rpc && rpc.SyncTime == 0)
            rpc.SyncTime = SyncTime;
        
        var payload = new NetPayload(
            message: message,
            senderId: senderId,
            receiverId: 0,
            packetOptions: PacketOption.None
        );
        _server.SendQueue.Enqueue(new PacketEvent(
            peerId: peerId,
            channel: channel,
            payload: payload
        ));
    }
    
    #endregion

    #region Receive
    
    private void HandleReceive(LobbyPlayer player, NetPayload payload)
    {
        if (payload.Messages == null)
            return;

        var neverRelay = payload.PacketOptions.HasFlag(PacketOption.OnlyFirstDegreeConnections);
        var isBroadcast = !neverRelay && payload.ReceiverId == 127;
        var isUnicast = !neverRelay && payload.ReceiverId is >= 1 and <= 126;
        var shouldProcess = payload.ReceiverId is 0 or 127; 
        
        LobbyPlayer? unicastPlayer = null;
        
        if (isUnicast)
            unicastPlayer = ConnectedPlayers.FirstOrDefault(p => p.ConnectionId == payload.ReceiverId);
        
        foreach (var message in payload.Messages)
        {
            // Message blacklist: clients may not send these
            switch (message)
            {
                case PlayerConnectedPacket:
                case PlayerDisconnectedPacket:
                case PlayerSortOrderPacket:
                case SyncTimePacket:
                case KickPlayerPacket:
                    // These are server-to-client only packets, client should never send these
                    // Either a misbehaving client or protocol error, kick the player
                    HandlePlayerSentIllegalMessage(player, message);
                    break;
            }

            // Relay messages if requested
            if (isBroadcast)
                SendToAllFrom(message, player.ConnectionId);
            else if (unicastPlayer != null)
                SendToFrom(message, unicastPlayer, player.ConnectionId);

            // Process and handle message if server was among the intended recipients
            if (!shouldProcess)
                continue;
            
            switch (message)
            {
                case PlayerIdentityPacket identityPacket:
                    var isIdentityInit = !player.HasIdentity;
                    player.SetPlayerIdentity(identityPacket);
                    GameMode.OnPlayerUpdate(player);
                    if (isIdentityInit)
                        _sortIndexUpdateNeeded = true;
                    SendToAllFrom(identityPacket, player.ConnectionId);
                    break;
                case PlayerAvatarPacket avatarPacket:
                    player.SetPlayerAvatar(avatarPacket.PlayerAvatar);
                    GameMode.OnPlayerUpdate(player);
                    break;
                case PlayerStatePacket statePacket:
                    player.SetPlayerState(statePacket.PlayerState);
                    GameMode.OnPlayerUpdate(player);
                    break;
                case PingPacket pingPacket:
                    SendTo(new PongPacket(pingPacket.PingTime), player);
                    SendTo(new SyncTimePacket(SyncTime), player);
                    break;
                case PongPacket pongPacket:
                    var isLatencyInit = !player.HasValidLatency;
                    player.LatencyAverage.Update(SyncTime - pongPacket.PingTime);
                    if (isLatencyInit)
                        _sortIndexUpdateNeeded = true;
                    break;
                case BaseMenuRpc menuRpc:
                    if (menuRpc is GetPlayersPermissionConfigurationRpc)
                        SendTo(new SetPlayersPermissionConfigurationRpc(GetPermissionsConfiguration()), player);
                    else
                        GameMode.HandleMenuRpc(menuRpc, player);
                    break;
                case BaseGameplayRpc gameplayRpc:
                    GameMode.HandleGameplayRpc(gameplayRpc, player);
                    break;
                case MpBeatmapPacket mpBeatmapPacket:
                    GameMode.HandleMpBeatmapPacket(mpBeatmapPacket, player);
                    break;
                case MpPlayerDataPacket mpPlayerDataPacket:
                    player.SetMpCorePlayerData(mpPlayerDataPacket);
                    GameMode.HandleMpPlayerData(mpPlayerDataPacket, player);
                    break;
                case NodePoseSyncStateNetSerializable:
                case NodePoseSyncStateDeltaNetSerializable:
                case StandardScoreSyncStateNetSerializable:
                case StandardScoreSyncStateDeltaNetSerializable:
                case GetMpPerPlayerPacket:
                case MpPerPlayerPacket:
                    // The messages are only for relaying; ignore them
                    break;
                default:
                    _logger?.Warning("Player {PlayerId} sent {PacketType}: no server handler implementation",
                        player.ConnectionId, message.GetType().Name);
                    break;
            }
        }
    }
    
    private void HandlePlayerSentIllegalMessage(LobbyPlayer player, INetSerializable message)
    {
        _logger?.Warning("Player {PlayerId} sent illegal message ({PacketType}), this may indicate a protocol error",
            player.ConnectionId, message.GetType().Name);
        
        KickPlayer(player, DisconnectedReason.Kicked);
    }
    
    #endregion

    #region Player connection events
    
    private void HandlePlayerConnect(LobbyPlayer newPlayer)
    {
        // Broadcast ping
        _lastPingTime = SyncTime;
        SendToAll(new PingPacket(_lastPingTime));
        
        // Broadcast new player join
        SendToAllExcluding(newPlayer.GetPlayerConnectedPacket(), excluding: newPlayer.ConnectionId);

        // Send existing player data to new player
        var otherPlayers = PlayerList
            .Where(p => p.ConnectionId != newPlayer.ConnectionId && !p.Disconnected)
            .ToList();
        
        foreach (var otherPlayer in otherPlayers)
        {
            SendTo(otherPlayer.GetPlayerConnectedPacket(), newPlayer);
            
            if (otherPlayer.SortIndex.HasValue)
                SendTo(otherPlayer.GetPlayerSortOrderPacket(), newPlayer);
            
            var identityPacket = otherPlayer.GetPlayerIdentityPacket();
            if (identityPacket != null)
                SendToFrom(identityPacket, newPlayer, otherPlayer.ConnectionId);
        }
        
        // Send host player data (hidden server player)
        SendToFrom(new PlayerIdentityPacket(
            new PlayerStateHash(BitMaskUtils.CreateBitMask128(["dedicated_server"])),
            new MultiplayerAvatarsData(new List<MultiplayerAvatarData>(), BitMask128.MinValue)
        ), newPlayer, 0);
        
        // Allow game mode to handle new player
        GameMode.OnPlayerConnect(newPlayer);
    }

    private void HandlePlayerDisconnect(LobbyPlayer player)
    {
        GameMode.OnPlayerDisconnect(player);
    }
    
    #endregion
    
    #region Lobby config data
    
    public BeatmapLevelSelectionMask GetBeatmapLevelSelectionMask()
    {
        return new BeatmapLevelSelectionMask(BeatmapDifficultyMask.All, GameplayModifierMask.All,
            new SongPackMask(BitMask256.MaxValue));
    }

    public GameplayServerConfiguration GetGameplayServerConfiguration()
    {
        return new GameplayServerConfiguration(MaxPlayerCount, DiscoveryPolicy.Public, InvitePolicy.AnyoneCanInvite,
            GameMode.GameplayServerMode, GameMode.SongSelectionMode, GetGameplayServerControlSettings());
    }

    public PlayersLobbyPermissionConfigurationNetSerializable GetPermissionsConfiguration()
    {
        var list = ConnectedPlayers.Select(player =>
            new PlayerLobbyPermissionConfigurationNetSerializable(
                userId: player.UserId!,
                isServerOwner: false,
                hasRecommendBeatmapsPermission: true,
                hasRecommendGameplayModifiersPermission: true,
                hasKickVotePermission: false,
                hasInvitePermission: true)
        ).ToList();
        return new PlayersLobbyPermissionConfigurationNetSerializable(list);
    }
    
    public bool SpectateAllowed => GameMode.AllowSpectate;
    public bool ModifierSelectionAllowed => GameMode.AllowModifierSelection;
    
    public GameplayServerControlSettings GetGameplayServerControlSettings()
    {
        var flags = GameplayServerControlSettings.None;
        if (SpectateAllowed)
            flags |= GameplayServerControlSettings.AllowModifierSelection;
        if (ModifierSelectionAllowed)
            flags |= GameplayServerControlSettings.AllowSpectate;
        return flags;
    }

    public const int DefaultMaxPlayerCount = 5;
    
    #endregion
}