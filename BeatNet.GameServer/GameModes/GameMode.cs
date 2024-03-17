using BeatNet.GameServer.Lobby;

namespace BeatNet.GameServer.GameModes;

public abstract class GameMode
{
    public readonly LobbyHost Host;
    
    public GameMode(LobbyHost host)
    {
        Host = host;
    }
    
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
    /// The player will not have a sort index yet, so will not be visible in the lobby.
    /// </summary>
    public abstract void OnPlayerConnect(LobbyPlayer player);

    /// <summary>
    /// Fired when a player has updated their state (e.g. state hash, avatar).
    /// </summary>
    public abstract void OnPlayerUpdate(LobbyPlayer player);

    /// <summary>
    /// Fired when a player has disconnected from the lobby.
    /// </summary>
    public abstract void OnPlayerDisconnect(LobbyPlayer player);
}