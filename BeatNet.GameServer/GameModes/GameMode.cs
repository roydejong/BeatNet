using BeatNet.GameServer.Lobby;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.GameServer.GameModes;

public abstract class GameMode
{
    public readonly LobbyHost Host;
    
    public GameMode(LobbyHost host)
    {
        Host = host;
    }
    
    public abstract GameplayServerMode GameplayServerMode { get; }
    public abstract SongSelectionMode SongSelectionMode { get; }
    public abstract bool AllowModifierSelection { get; }
    public abstract bool AllowSpectate { get; }

    /// <summary>
    /// Gets the name/description of the game mode.
    /// Users may see this in the server browser.
    /// </summary>
    public abstract string GetName();
    
    /// <summary>
    /// Called when the lobby is started or transitioned to this game mode.
    /// This should cause all players to be forcibly returned to the lobby.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// General update tick for the game mode.
    /// Called every second.
    /// </summary>
    public abstract void Tick();

    /// <summary>
    /// Fired when a player connects to the lobby.
    /// The player may not have a valid state or sort index yet.
    /// </summary>
    public abstract void OnPlayerConnect(LobbyPlayer player);

    /// <summary>
    /// Fired when a player "spawns" in the lobby; when their sort index gets assigned.
    /// The player will now have a valid state, sort index and latency.
    /// </summary>
    public abstract void OnPlayerSpawn(LobbyPlayer player);

    /// <summary>
    /// Fired when a player has updated their state (identity, avatar, state changes).
    /// </summary>
    public abstract void OnPlayerUpdate(LobbyPlayer player);

    /// <summary>
    /// Fired when a player has disconnected from the lobby.
    /// </summary>
    public abstract void OnPlayerDisconnect(LobbyPlayer player);

    /// <summary>
    /// Handles a menu / lobby RPC from a player.
    /// </summary>
    public abstract void HandleMenuRpc(BaseMenuRpc menuRpc, LobbyPlayer player);

    /// <summary>
    /// Handles a gameplay RPC from a player.
    /// </summary>
    /// <param name="gameplayRpc"></param>
    /// <param name="player"></param>
    public abstract void HandleGameplayRpc(BaseGameplayRpc gameplayRpc, LobbyPlayer player);
}