using BeatNet.GameServer.Lobby;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

namespace BeatNet.GameServer.GameModes;

public class QuickPlayGameMode : GameMode
{
    public MultiplayerGameState GameState { get; private set; }
    
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
        GameState = MultiplayerGameState.Lobby;
    }
    
    public override void Tick()
    {
    }

    public override void OnPlayerConnect(LobbyPlayer player)
    {
    }
    
    public override void OnPlayerSpawn(LobbyPlayer player)
    {
    }
    
    public override void OnPlayerUpdate(LobbyPlayer player)
    {
    }

    public override void OnPlayerDisconnect(LobbyPlayer player)
    {
    }

    public override void HandleMenuRpc(BaseMenuRpc menuRpc, LobbyPlayer player)
    {
        switch (menuRpc)
        {
            case GetMultiplayerGameStateRpc:
                player.Send(new SetMultiplayerGameStateRpc(GameState));
                break;
            case GetRecommendedBeatmapRpc:
                player.Send(new ClearRecommendedBeatmapRpc());
                break;
            case GetRecommendedGameplayModifiersRpc:
                player.Send(new ClearRecommendedGameplayModifiersRpc());
                break;
        }
    }

    public override void HandleGameplayRpc(BaseGameplayRpc gameplayRpc, LobbyPlayer player)
    {
        
    }
}