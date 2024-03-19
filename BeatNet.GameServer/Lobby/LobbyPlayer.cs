using BeatNet.GameServer.Util;
using BeatNet.Lib.BeatSaber;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.BeatSaber.Generated.Packet;
using BeatNet.Lib.BeatSaber.Util;
using BeatNet.Lib.Net;

namespace BeatNet.GameServer.Lobby;

public class LobbyPlayer : IConnectedPlayer
{
    public readonly LobbyHost LobbyHost;
    public readonly uint PeerId;
    public readonly byte ConnectionId;

    public byte Id => ConnectionId;
    
    public byte? RemoteConnectionId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? PlayerSessionId { get; set; }
    public int? SortIndex { get; set; }
    public PlayerStateHash? State { get; set; }
    public MultiplayerAvatarsData? Avatars { get; set; }
    private byte[]? PublicEncryptionKey { get; set; }
    private byte[]? Random { get; set; }
    public bool Disconnected { get; set; }

    public bool PendingConnection => UserId == null;
    public bool PendingSpawn => SortIndex == null;

    private PlayerIdentityPacket? _playerIdentityPacket = null;
    
    public readonly LongRollingAverage LatencyAverage = new(30);
    public bool HasValidLatency => LatencyAverage.HasValue;

    public LobbyPlayer(LobbyHost lobbyHost, uint peerId, byte connectionId)
    {
        LobbyHost = lobbyHost;
        PeerId = peerId;
        ConnectionId = connectionId;
        
        RemoteConnectionId = null;
        UserId = null;
        UserName = null;
        PlayerSessionId = null;
        SortIndex = null;
        State = null;
        Disconnected = false;
        PublicEncryptionKey = null;
        Random = null;

        _playerIdentityPacket = null;
    }

    #region State updates
    
    public void SetConnectionRequest(ConnectionRequest connectEventConnectionRequest)
    {
        UserId = connectEventConnectionRequest.UserId;
        UserName = connectEventConnectionRequest.UserName;
        PlayerSessionId = connectEventConnectionRequest.PlayerSessionId;
    }
    
    public void SetPlayerIdentity(PlayerIdentityPacket identityPacket)
    {
        State = identityPacket.PlayerState;
        Avatars = identityPacket.PlayerAvatar;
        PublicEncryptionKey = identityPacket.PublicEncryptionKey.GetData(true);
        Random = identityPacket.Random.GetData(true);
        
        _playerIdentityPacket = identityPacket;
    }

    public void SetPlayerState(PlayerStateHash state)
    {
        State = state;
    }

    public void SetPlayerAvatar(MultiplayerAvatarsData avatarsData)
    {
        Avatars = avatarsData;
    }
    
    #endregion

    #region State helpers

    /// <summary>
    /// The client sets this state on session start. It should always be set.
    /// </summary>
    public bool StateMultiplayerSession => State?.Contains("multiplayer_session") ?? false;

    /// <summary>
    /// The client sets this state if it is a player (not a spectator or dedicated server).
    /// </summary>
    public bool StatePlayer => State?.Contains("player") ?? false;

    /// <summary>
    /// The client does not set this state normally.
    /// It would be set the client is a dedicated server (not a player or spectator).
    /// </summary>
    public bool StateDedicatedServer => State?.Contains("dedicated_server") ?? false;

    /// <summary>
    /// The client sets this state if it is spectating (not a player or dedicated server).
    /// </summary>
    public bool StateSpectating => State?.Contains("spectating") ?? false;

    /// <summary>
    /// The client sets this state on session start.
    /// This state is removed if the client wants to spectate.
    /// </summary>
    public bool StateWantsToPlayNextLevel => State?.Contains("wants_to_play_next_level") ?? false;

    /// <summary>
    /// The client sets this state if the application is paused.
    /// </summary>
    public bool StateBackgrounded => State?.Contains("backgrounded") ?? false;

    /// <summary>
    /// Indicates whether the player is actively participating in the level.
    /// The client sets this state when the lobby countdown is finished, if "wants_to_play_next_level" is also set.
    /// The client removes this state if gameplay ends for any reason (finish, fail, give up, late load/connect, etc.).
    /// </summary>
    public bool StateIsActive => State?.Contains("is_active") ?? false;

    /// <summary>
    /// The clients set this state if "is_active" was set at the start of gameplay.
    /// This state is removed if the player connects / loads late.
    /// </summary>
    public bool StateWasActiveAtLevelStart => State?.Contains("was_active_at_level_start") ?? false;

    /// <summary>
    /// The client sets this state if its MenuRpcManager is enabled (when in lobby).
    /// </summary>
    public bool StateInMenu => State?.Contains("in_menu") ?? false;

    /// <summary>
    /// The client sets this state if its GameplayRpcManager is enabled (when in gameplay).
    /// </summary>
    public bool StateInGameplay => State?.Contains("in_gameplay") ?? false;

    /// <summary>
    /// The client sets this state once it has finished a level during gameplay.
    /// </summary>
    public bool StateFinishedLevel => State?.Contains("finished_level") ?? false;

    /// <summary>
    /// Special state for dedicated servers that are shutting down.
    /// </summary>
    public bool StateTerminating => State?.Contains("terminating") ?? false;

    /// <summary>
    /// State added by all MultiplayerCore users.
    /// </summary>
    public bool StateModded => State?.Contains("modded") ?? false;

    #endregion

    #region Packet helpers
    
    public PlayerConnectedPacket GetPlayerConnectedPacket()
    {
        return new PlayerConnectedPacket(ConnectionId, UserId!, UserName!, false);
    }

    public PlayerSortOrderPacket GetPlayerSortOrderPacket()
    {
        return new PlayerSortOrderPacket(UserId!, SortIndex!.Value);
    }

    public PlayerIdentityPacket? GetPlayerIdentityPacket()
    {
        return _playerIdentityPacket;
    }
    
    #endregion

    #region Send util

    public void Send(BaseRpc rpc)
    {
        LobbyHost.SendTo(rpc, this);
    }

    #endregion
}