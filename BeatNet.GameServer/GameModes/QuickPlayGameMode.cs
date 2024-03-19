using BeatNet.GameServer.GameModes.Common;
using BeatNet.GameServer.GameModes.Models;
using BeatNet.GameServer.Lobby;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

namespace BeatNet.GameServer.GameModes;

public class QuickPlayGameMode : GameMode
{
    public MultiplayerGameState GameState { get; private set; }

    private readonly LevelVoting _levelVoting;
    
    public QuickPlayGameMode(LobbyHost host) : base(host)
    {
        _levelVoting = new();
        _levelVoting.TopVotedBeatmapChanged += HandleTopVotedBeatmapChanged;
        _levelVoting.TopVotedModifiersChanged += HandleTopVotedModifiersChanged;
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
        _levelVoting.Reset();
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
        _levelVoting.ClearPlayer(player);
    }

    public override void HandleMenuRpc(BaseMenuRpc menuRpc, LobbyPlayer player)
    {
        switch (menuRpc)
        {
            // Game state
            case GetMultiplayerGameStateRpc:
                player.Send(new SetMultiplayerGameStateRpc(GameState));
                break;
            
            // Voting
            case GetSelectedBeatmapRpc:
                var selectedBeatmap = _levelVoting.TopVotedBeatmap;
                if (selectedBeatmap != null)
                    player.Send(new SetSelectedBeatmapRpc(selectedBeatmap.ToBeatmapKey()));
                else
                    player.Send(new ClearSelectedBeatmapRpc());
                break;
            case GetSelectedGameplayModifiersRpc:
                var selectedModifiers = _levelVoting.TopVotedModifiers;
                player.Send(new SetSelectedGameplayModifiersRpc(selectedModifiers ?? DefaultModifiers));
                break;
            case RecommendBeatmapRpc recommendBeatmapRpc:
                if (recommendBeatmapRpc.Key != null)
                    _levelVoting.SetRecommendedBeatmap(player, BeatmapLevel.FromBeatmapKey(recommendBeatmapRpc.Key));
                break;
            case ClearRecommendedBeatmapRpc:
                _levelVoting.ClearRecommendedBeatmap(player);
                break;
            case RecommendGameplayModifiersRpc recommendGameplayModifiersRpc:
                if (recommendGameplayModifiersRpc.GameplayModifiers != null)
                    _levelVoting.SetRecommendedModifiers(player, recommendGameplayModifiersRpc.GameplayModifiers);
                break;
            case ClearRecommendedGameplayModifiersRpc:
                _levelVoting.ClearRecommendedModifiers(player);
                break;
            
            // Countdown / level start
            case GetCountdownEndTimeRpc:
                player.Send(new CancelCountdownRpc());
                break;
            case GetStartedLevelRpc:
                break;
        }
    }

    public override void HandleGameplayRpc(BaseGameplayRpc gameplayRpc, LobbyPlayer player)
    {
        
    }

    private void HandleTopVotedBeatmapChanged(BeatmapLevel? level)
    {
        if (level != null)
            Host.SendToAll(new SetSelectedBeatmapRpc(level.ToBeatmapKey()));
        else
            Host.SendToAll(new ClearSelectedBeatmapRpc());
    }

    private void HandleTopVotedModifiersChanged(GameplayModifiers? modifiers)
    {
        Host.SendToAll(new SetSelectedGameplayModifiersRpc(modifiers ?? DefaultModifiers));
    }

    public static GameplayModifiers DefaultModifiers =>
        new(EnergyType.Bar, true, false, false, EnabledObstacleType.All,
            false, false, false, false, false, SongSpeed.Normal,
            false, false, false, false);
}