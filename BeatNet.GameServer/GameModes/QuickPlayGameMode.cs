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
    private readonly Countdown _countdown;
    
    public QuickPlayGameMode(LobbyHost host) : base(host)
    {
        _levelVoting = new();
        _countdown = new(host);
        
        _levelVoting.TopVotedBeatmapChanged += HandleTopVotedBeatmapChanged;
        _levelVoting.TopVotedModifiersChanged += HandleTopVotedModifiersChanged;
        
        _countdown.CountdownEndTimeSet += HandleCountdownEndTimeSet;
        _countdown.CountdownCancelled += HandleCountdownCancelled;
        _countdown.CountdownLockedIn += HandleCountdownLockedIn;
        _countdown.CountdownFinished += HandleCountdownFinished;
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
        _countdown.Update();
    }

    public override void OnPlayerConnect(LobbyPlayer player)
    {
        _countdown.SetPlayerReady(player, false);
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
        _countdown.SetPlayerReady(player, false);
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
                if (selectedBeatmap != null)
                    player.Send(new GetIsEntitledToLevelRpc(selectedBeatmap.LevelId));
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
            case SetIsReadyRpc setPlayerReadyRpc:
                _countdown.SetPlayerReady(player, setPlayerReadyRpc.IsReady ?? false);
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
        
        _countdown.SetSelectedLevel(level, _levelVoting.TopVotedModifiers ?? DefaultModifiers);
        
        if (level != null)
            Host.SendToAll(new GetIsEntitledToLevelRpc(level.LevelId));
    }

    private void HandleTopVotedModifiersChanged(GameplayModifiers? modifiers)
    {
        Host.SendToAll(new SetSelectedGameplayModifiersRpc(modifiers ?? DefaultModifiers));
    }
    
    private void HandleCountdownFinished()
    {
        GameState = MultiplayerGameState.Game;
        Host.SendToAll(new SetMultiplayerGameStateRpc(MultiplayerGameState.Game));
    }

    private void HandleCountdownLockedIn(BeatmapLevel level, GameplayModifiers? modifiers, long levelStartTime)
    {
        Host.SendToAll(new StartLevelRpc(level.ToBeatmapKey(), modifiers ?? DefaultModifiers, levelStartTime));
    }

    private void HandleCountdownEndTimeSet(long endTime)
    {
        Host.SendToAll(new SetCountdownEndTimeRpc(endTime));
    }

    private void HandleCountdownCancelled()
    {
        if (GameState != MultiplayerGameState.Lobby)
            return;
        
        Host.SendToAll(new CancelCountdownRpc());
        Host.SendToAll(new CancelLevelStartRpc());
    }

    public static GameplayModifiers DefaultModifiers =>
        new(EnergyType.Bar, true, false, false, EnabledObstacleType.All,
            false, false, false, false, false, SongSpeed.Normal,
            false, false, false, false);
}