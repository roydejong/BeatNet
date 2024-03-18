using BeatNet.GameServer.Lobby;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.GameServer.GameModes;

public class QuickPlayGameMode : GameMode
{
    public QuickPlayGameMode(LobbyHost host) : base(host)
    {
    }

    public override GameplayServerMode GameplayServerMode => GameplayServerMode.Countdown;
    public override SongSelectionMode SongSelectionMode => SongSelectionMode.Vote;
    public override bool AllowModifierSelection => true;
    public override bool AllowSpectate => true;

    public override string GetName()
    {
        return "WIPDEV Quick Play";
    }

    public override void Reset()
    {
    }
    
    public override void Tick()
    {
    }

    public override void OnPlayerConnect(LobbyPlayer player)
    {
    }
    
    public override void OnPlayerUpdate(LobbyPlayer player)
    {
    }

    public override void OnPlayerDisconnect(LobbyPlayer player)
    {
    }
}