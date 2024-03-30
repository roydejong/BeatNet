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

    public bool HasIdentity => _playerIdentityPacket != null;

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
        PublicEncryptionKey = identityPacket.PublicEncryptionKey.CopyData(true);
        Random = identityPacket.Random.CopyData(true);
        
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
    /// Set on multiplayer session start. This should always be set in a client connection.
    /// </summary>
    public bool StateMultiplayerSession => State?.Contains("multiplayer_session") ?? false;

    /// <summary>
    /// Indicates session type: Player.
    /// This should ALWAYS be set in a client connection.
    /// </summary>
    public bool StatePlayer => State?.Contains("player") ?? false;

    /// <summary>
    /// Indicates session type: Dedicated server.
    /// The client does NOT set this state normally.
    /// </summary>
    public bool StateDedicatedServer => State?.Contains("dedicated_server") ?? false;

    /// <summary>
    /// Indicates session type: Spectating.
    /// The client does NOT set this state normally.
    /// </summary>
    public bool StateSpectating => State?.Contains("spectating") ?? false;

    /// <summary>
    /// Indicates that the game is paused / backgrounded for this player.
    /// </summary>
    public bool StateBackgrounded => State?.Contains("backgrounded") ?? false;
    
    /// <summary>
    /// Indicates that this server is shutting down (lobby host only).
    /// If set, clients will disconnect and show an appropriate error.
    /// </summary>
    public bool StateTerminating => State?.Contains("terminating") ?? false;

    /// <summary>
    /// Indicates that the player wants to play the next level.
    /// If not set, the player wants to spectate.
    /// </summary>
    public bool StateWantsToPlayNextLevel => State?.Contains("wants_to_play_next_level") ?? false;

    /// <summary>
    /// Indicates that the player is currently participating in gameplay.
    /// If not set, the player is spectating, or has finished/ended the level in some way.
    /// </summary>
    public bool StateIsActive => State?.Contains("is_active") ?? false;

    /// <summary>
    /// Indicates that player had the `is_active` state when the gameplay level begun.
    /// </summary>
    public bool StateWasActiveAtLevelStart => State?.Contains("was_active_at_level_start") ?? false;

    /// <summary>
    /// Indicates that the player has fully finished a level.
    /// </summary>
    public bool StateFinishedLevel => State?.Contains("finished_level") ?? false;

    /// <summary>
    /// Indicates that the client's `MenuRpcManager` is active and Menu RPCs can be handled.
    /// </summary>
    public bool StateInMenu => State?.Contains("in_menu") ?? false;

    /// <summary>
    /// Indicates that the client's `GameplayRpcManager` is active and Gameplay RPCs can be handled.
    /// </summary>
    public bool StateInGameplay => State?.Contains("in_gameplay") ?? false;

    /// <summary>
    /// Indicates a modded client (MultiplayerCore).
    /// </summary>
    public bool StateModded => State?.Contains("modded") ?? false;

    #endregion

    #region Packet helpers
    
    public PlayerConnectedPacket GetPlayerConnectedPacket() => new(ConnectionId, UserId!, UserName!, false);
    public PlayerSortOrderPacket GetPlayerSortOrderPacket() => new(UserId!, SortIndex!.Value);
    public PlayerIdentityPacket? GetPlayerIdentityPacket() => _playerIdentityPacket;
    
    #endregion

    #region Send util

    public void Send(BaseRpc rpc)
    {
        LobbyHost.SendTo(rpc, this);
    }

    #endregion
}