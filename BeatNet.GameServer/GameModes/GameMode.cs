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
}