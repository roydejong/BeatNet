using BeatNet.GameServer.GameModes.Common;
using BeatNet.GameServer.GameModes.Models;
using BeatNet.GameServer.Lobby;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Gameplay;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

namespace BeatNet.GameServer.GameModes;

public class QuickPlayGameMode : GameMode
{
    public MultiplayerGameState GameState { get; private set; }

    private readonly VoteManager _voteManager;
    private readonly CountdownManager _countdownManager;
    private readonly GameplayManager _gameplayManager;

    private BeatmapLevel? _currentLevel;
    private GameplayModifiers? _currentModifiers;
    private long? _levelStartTime;
    
    public QuickPlayGameMode(LobbyHost host) : base(host)
    {
        _voteManager = new();
        _countdownManager = new(host);
        _gameplayManager = new(host);
        
        _voteManager.TopVotedBeatmapChanged += HandleTopVotedBeatmapChanged;
        _voteManager.TopVotedModifiersChanged += HandleTopVotedModifiersChanged;
        
        _countdownManager.CountdownEndTimeSet += HandleCountdownManagerEndTimeSet;
        _countdownManager.CountdownCancelled += HandleCountdownManagerCancelled;
        _countdownManager.CountdownLockedIn += HandleCountdownManagerLockedIn;
        _countdownManager.CountdownFinished += HandleCountdownManagerFinished;
        
        _gameplayManager.GameplayCancelledEvent += HandleGameplayCancelled;
        _gameplayManager.GameplayFinishedEvent += HandleGameplayFinished;
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
        
        _voteManager.Reset();
        _countdownManager.Reset();
        _gameplayManager.StopImmediately(false);
        
        _currentLevel = null;
        _currentModifiers = null;
        _levelStartTime = null;
    }
    
    public override void Tick()
    {
        _countdownManager.Update();
        _gameplayManager.Update();
    }

    public override void OnPlayerConnect(LobbyPlayer player)
    {
        _countdownManager.SetPlayerReady(player, false);
    }
    
    public override void OnPlayerSpawn(LobbyPlayer player)
    {
    }
    
    public override void OnPlayerUpdate(LobbyPlayer player)
    {
        if (!player.StateWantsToPlayNextLevel)
        {
            // Player wants to spectate, not play
            _voteManager.ClearPlayer(player);
            _countdownManager.SetPlayerReady(player, false);
        }
    }

    public override void OnPlayerDisconnect(LobbyPlayer player)
    {
        _voteManager.ClearPlayer(player);
        _countdownManager.SetPlayerReady(player, false);
        _gameplayManager.RemovePlayer(player);
    }

    public override void HandleMenuRpc(BaseMenuRpc menuRpc, LobbyPlayer player)
    {
        switch (menuRpc)
        {
            // Game state
            case GetMultiplayerGameStateRpc:
            {
                if (GameState == MultiplayerGameState.Lobby && _countdownManager.IsLockedIn &&
                    _countdownManager.CountdownTimeRemaining < 1f)
                {
                    // Prevent possible race condition (seen on local dev):
                    // Assume we're in gameplay if countdown is very close to finishing
                    player.Send(new SetMultiplayerGameStateRpc(MultiplayerGameState.Game));
                }
                else
                {
                    player.Send(new SetMultiplayerGameStateRpc(GameState));
                }

                break;
            }
            case SetIsInLobbyRpc setIsInLobbyRpc:
            {
                if (setIsInLobbyRpc.IsBack ?? false)
                {
                    // Player has (returned) to lobby, ensure their UI state is synced up
                    var shownLevel = _voteManager.TopVotedBeatmap;
                    var shownModifiers = _voteManager.TopVotedModifiers ?? DefaultModifiers;
                    if (shownLevel != null)
                        player.Send(new SetSelectedBeatmapRpc(shownLevel.ToBeatmapKey()));
                    else
                        player.Send(new ClearSelectedBeatmapRpc());
                    player.Send(new SetSelectedGameplayModifiersRpc(shownModifiers));
                    player.Send(new GetRecommendedBeatmapRpc());
                    player.Send(new GetRecommendedGameplayModifiersRpc());
                    // (They'll ask for countdown stuff manually but I've seen some issues with recommends)
                }
                break;
            }

            // Voting
            case GetSelectedBeatmapRpc:
            {
                var selectedBeatmap = _voteManager.TopVotedBeatmap;
                if (selectedBeatmap != null)
                    player.Send(new SetSelectedBeatmapRpc(selectedBeatmap.ToBeatmapKey()));
                else
                    player.Send(new ClearSelectedBeatmapRpc());
                if (selectedBeatmap != null)
                    player.Send(new GetIsEntitledToLevelRpc(selectedBeatmap.LevelId));
                break;
            }
            case GetSelectedGameplayModifiersRpc:
            {
                var selectedModifiers = _voteManager.TopVotedModifiers;
                player.Send(new SetSelectedGameplayModifiersRpc(selectedModifiers ?? DefaultModifiers));
                break;
            }
            case RecommendBeatmapRpc recommendBeatmapRpc:
            {
                if (recommendBeatmapRpc.Key != null)
                    _voteManager.SetRecommendedBeatmap(player, BeatmapLevel.FromBeatmapKey(recommendBeatmapRpc.Key));
                break;
            }
            case ClearRecommendedBeatmapRpc:
            {
                _voteManager.ClearRecommendedBeatmap(player);
                break;
            }
            case RecommendGameplayModifiersRpc recommendGameplayModifiersRpc:
            {
                if (recommendGameplayModifiersRpc.GameplayModifiers != null)
                    _voteManager.SetRecommendedModifiers(player, recommendGameplayModifiersRpc.GameplayModifiers);
                break;
            }
            case ClearRecommendedGameplayModifiersRpc:
            {
                _voteManager.ClearRecommendedModifiers(player);
                break;
            }
            case SetIsReadyRpc setPlayerReadyRpc:
            {
                _countdownManager.SetPlayerReady(player, setPlayerReadyRpc.IsReady ?? false);
                break;
            }
            
            // Countdown / level start
            case GetCountdownEndTimeRpc:
            {
                if (_countdownManager.IsCountingDown)
                    player.Send(new SetCountdownEndTimeRpc(_countdownManager.CountdownEndTime!));
                else
                    player.Send(new CancelCountdownRpc());
                break;
            }
            case GetStartedLevelRpc:
            {
                if (_countdownManager.IsLockedIn || _gameplayManager.Started)
                    player.Send(new StartLevelRpc(_currentLevel!.ToBeatmapKey(), _currentModifiers ?? DefaultModifiers, 
                        _levelStartTime!));
                else
                    player.Send(new CancelLevelStartRpc());
                break;
            }
        }
    }

    public override void HandleGameplayRpc(BaseGameplayRpc gameplayRpc, LobbyPlayer player)
    {
        _gameplayManager.HandleGameplayRpc(gameplayRpc, player);
    }

    private void HandleTopVotedBeatmapChanged(BeatmapLevel? level)
    {
        if (_countdownManager.IsLockedIn || _gameplayManager.Started)
            return;
        
        if (level != null)
            Host.SendToAll(new SetSelectedBeatmapRpc(level.ToBeatmapKey()));
        else
            Host.SendToAll(new ClearSelectedBeatmapRpc());
        
        if (level != null)
            Host.SendToAll(new GetIsEntitledToLevelRpc(level.LevelId));

        UpdateCountdownSelectedLevel();
    }

    private void HandleTopVotedModifiersChanged(GameplayModifiers? modifiers)
    {
        if (_countdownManager.IsLockedIn || _gameplayManager.Started)
            return;
        
        Host.SendToAll(new SetSelectedGameplayModifiersRpc(modifiers ?? DefaultModifiers));

        UpdateCountdownSelectedLevel();
    }

    private void UpdateCountdownSelectedLevel()
    {
        _countdownManager.SetSelectedLevel(_voteManager.TopVotedBeatmap, _voteManager.TopVotedModifiers ?? DefaultModifiers);
    }
    
    private void HandleCountdownManagerFinished()
    {
        GameState = MultiplayerGameState.Game;
        _gameplayManager.Start();
    }

    private void HandleCountdownManagerLockedIn(BeatmapLevel level, GameplayModifiers? modifiers, long levelStartTime)
    {
        Host.SendToAll(new StartLevelRpc(level.ToBeatmapKey(), modifiers ?? DefaultModifiers, levelStartTime));
        
        _currentLevel = level;
        _currentModifiers = modifiers;
        _levelStartTime = levelStartTime;
    }

    private void HandleCountdownManagerEndTimeSet(long endTime)
    {
        if (GameState != MultiplayerGameState.Lobby)
            return;
        
        Host.SendToAll(new SetCountdownEndTimeRpc(endTime));
    }

    private void HandleCountdownManagerCancelled()
    {
        if (GameState != MultiplayerGameState.Lobby)
            return;
        
        Host.SendToAll(new CancelCountdownRpc());
        Host.SendToAll(new CancelLevelStartRpc());
        
        Host.SendToAll(new GetRecommendedBeatmapRpc());
        Host.SendToAll(new GetRecommendedGameplayModifiersRpc());
    }

    private void HandleGameplayCancelled()
    {
        if (GameState != MultiplayerGameState.Game)
            return;

        GameState = MultiplayerGameState.Lobby;
        
        Host.SendToAll(new ReturnToMenuRpc());
        Host.SendToAll(new SetMultiplayerGameStateRpc(MultiplayerGameState.Lobby));
        
        _currentLevel = null;
        _currentModifiers = null;
    }

    private void HandleGameplayFinished()
    {
        if (GameState != MultiplayerGameState.Game)
            return;

        GameState = MultiplayerGameState.Lobby;
        
        Host.SendToAll(new ReturnToMenuRpc());
        Host.SendToAll(new SetMultiplayerGameStateRpc(MultiplayerGameState.Lobby));
        
        _currentLevel = null;
        _currentModifiers = null;
    }

    public static GameplayModifiers DefaultModifiers =>
        new(EnergyType.Bar, true, false, false, EnabledObstacleType.All,
            false, false, false, false, false, SongSpeed.Normal,
            false, false, false, false);
}