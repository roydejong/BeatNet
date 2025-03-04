using BeatNet.GameServer.Lobby;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Gameplay;
using BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;
using Serilog;
using Serilog.Core;

namespace BeatNet.GameServer.GameModes.Common;

public class GameplayManager
{
    private readonly LobbyHost _host;

    public GameplayState State { get; private set; }
    public bool Started => State is not GameplayState.NotStarted;
    public string? GameplaySessionId { get; private set; }

    private List<LobbyPlayer> _playersAtLevelStart;
    private List<LobbyPlayer> _playersRemaining;
    private readonly Dictionary<byte, PlayerSpecificSettingsNetSerializable> _playerSceneSettings;
    private readonly List<byte> _playerSongReady;
    private readonly Dictionary<byte, MultiplayerLevelCompletionResults> _results;

    private long? _sceneLoadStartTime;
    private long? _songLoadStartTime;
    private long? _songStartTime;
    private long? _firstCompletionTime;
    private long? _outroStartTime;

    public event Action? GameplayFinishedEvent;
    public event Action? GameplayCancelledEvent;

    public GameplayManager(LobbyHost host)
    {
        _host = host;

        State = GameplayState.NotStarted;
        GameplaySessionId = null;

        _playersAtLevelStart = new();
        _playersRemaining = new();
        _playerSceneSettings = new();
        _playerSongReady = new();
        _results = new();
    }

    private void ResetState()
    {
        State = GameplayState.NotStarted;
        GameplaySessionId = null;

        _playersAtLevelStart.Clear();
        _playersRemaining.Clear();
        _playerSceneSettings.Clear();
        _playerSongReady.Clear();
        _results.Clear();

        _sceneLoadStartTime = null;
        _songLoadStartTime = null;
        _songStartTime = null;
        _firstCompletionTime = null;
        _outroStartTime = null;
    }

    public void Start()
    {
        if (Started)
            return;

        ResetState();

        _playersAtLevelStart = _host.ConnectedPlayers
            .Where(p => p.StateWantsToPlayNextLevel || p.StateIsActive)
            .ToList();
        _playersRemaining = _playersAtLevelStart.ToList();

        State = GameplayState.SceneSyncStart;
        _sceneLoadStartTime = _host.SyncTime;
    }

    public void StopImmediately(bool finished)
    {
        if (!Started)
            return;

        ResetState();

        if (!finished)
            _host.SendToAll(ReturnToMenuRpc.Instance);

        if (finished)
            GameplayFinishedEvent?.Invoke();
        else
            GameplayCancelledEvent?.Invoke();
    }

    public void Update()
    {
        if (!Started)
            return;

        switch (State)
        {
            case GameplayState.SceneSyncStart:
            {
                // Clients are transitioning to the gameplay scene, and send their settings when ready
                var anyPlayersRemaining = _playersRemaining.Any(p => p.StateIsActive);
                var haveSettingsForAllPlayers = _playersAtLevelStart
                    .All(p => _playerSceneSettings.ContainsKey(p.Id));
                var shouldForceStart = _host.SyncTime - _sceneLoadStartTime >= SceneLoadTimeoutMs;

                if (haveSettingsForAllPlayers || shouldForceStart || !anyPlayersRemaining)
                {
                    // All clients loaded scene (or timeout reached)
                    GameplaySessionId = Guid.NewGuid().ToString();
                    
                    var settingsSz = new PlayerSpecificSettingsAtStartNetSerializable(
                        _playerSceneSettings.Values.ToList()
                    );

                    // Notify any late players, rebuild active players
                    var latePlayers = _host.ConnectedPlayers
                        .Where(p => !_playerSceneSettings.ContainsKey(p.Id) && p.StateWasActiveAtLevelStart)
                        .ToList();

                    foreach (var latePlayer in latePlayers)
                    {
                        _host.SendToAll(new SetPlayerDidConnectLateRpc(latePlayer.UserId, settingsSz,
                            GameplaySessionId));
                        _playersRemaining.Remove(latePlayer);
                    }
                    
                    // Finish the scene sync
                    _host.SendToAll(new SetGameplaySceneSyncFinishedRpc(settingsSz, GameplaySessionId!));

                    // Move on to song sync start immediately
                    Log.Logger.Warning("GAMEPLAY: Scenes loaded! Moving to song load...");
                    State = GameplayState.SongSyncStart;
                    _songLoadStartTime = _host.SyncTime;
                    Update();
                    return;
                }
                
                // Not all players ready yet; ensure they're transitioning and ask for their settings
                var nonReadyPlayers = _playersAtLevelStart
                    .Where(p => !_playerSceneSettings.ContainsKey(p.Id))
                    .ToList();

                foreach (var nonReadyPlayer in nonReadyPlayers)
                    nonReadyPlayer.Send(GetGameplaySceneReadyRpc.Instance);
                
                Log.Logger.Warning("GAMEPLAY: Waiting for scene loads...");
                break;
            }
            case GameplayState.SongSyncStart:
            {
                // Clients are in the lobby and we're waiting for all of them to finish loading the song / level
                var anyPlayersRemaining = _playersRemaining.Any(p => p.StateIsActive);
                var haveSongReadyForAllPlayers = _playersAtLevelStart
                    .All(p => _playerSongReady.Contains(p.Id));
                var shouldForceStart = _host.SyncTime - _songLoadStartTime >= AudioLoadTimeoutMs;
                
                if (haveSongReadyForAllPlayers || shouldForceStart || !anyPlayersRemaining)
                {
                    // All clients loaded song (or timeout reached)
                    var maxPlayerLatency = _playersAtLevelStart
                        .Select(p => p.LatencyAverage.CurrentAverage)
                        .Max();
                    _songStartTime = _host.SyncTime + FixedSongStartTimeDelayMs + (maxPlayerLatency * 2);
                    
                    // Notify any late players, rebuild active players
                    var latePlayers = _host.ConnectedPlayers
                        .Where(p => !_playerSongReady.Contains(p.Id) && p.StateWasActiveAtLevelStart)
                        .ToList();

                    if (latePlayers.Count > 0)
                    {
                        var settingsSz = new PlayerSpecificSettingsAtStartNetSerializable(
                            _playerSceneSettings.Values.ToList()
                        );
                        foreach (var latePlayer in latePlayers)
                        {
                            _host.SendToAll(new SetPlayerDidConnectLateRpc(latePlayer.UserId, settingsSz,
                                GameplaySessionId));
                            _playersRemaining.Remove(latePlayer);
                        }
                    }

                    // Start the song
                    Log.Logger.Warning("GAMEPLAY: Song loaded! Setting start time...");
                    _host.SendToAll(new SetSongStartTimeRpc(_songStartTime));

                    // Move on to gameplay
                    State = GameplayState.Gameplay;
                    return;
                }
                
                // Not all players ready yet; ask for their song ready status
                var nonReadyPlayers = _playersAtLevelStart
                    .Where(p => !_playerSongReady.Contains(p.Id))
                    .ToList();
                
                foreach (var nonReadyPlayer in nonReadyPlayers)
                    nonReadyPlayer.Send(GetGameplaySongReadyRpc.Instance);
                
                Log.Logger.Warning("GAMEPLAY: Waiting for song loads...");
                break;
            }
            case GameplayState.Gameplay:
            {
                var activePlayers = _playersRemaining
                    .Where(p => p.StateIsActive && p.StateInGameplay)
                    .ToList();
                
                var anyCompletionResults = _results.Values.Any(r =>
                    r.PlayerLevelEndReason == MultiplayerPlayerLevelEndReason.Cleared);
                
                if (_firstCompletionTime == null && anyCompletionResults)
                    _firstCompletionTime = _host.SyncTime;
                
                var resultsDidTimeOut = _firstCompletionTime != null &&
                                        _host.SyncTime - _firstCompletionTime >= ResultWaitTimeoutMs;
                
                if (activePlayers.Count == 0 || resultsDidTimeOut)
                {
                    // All players finished or timeout reached
                    if (anyCompletionResults)
                    {
                        // Outro plays when we have any completion results
                        State = GameplayState.Outro;
                        _outroStartTime = _host.SyncTime;
                        Log.Logger.Warning("GAMEPLAY: Finished, playing outro");
                    }
                    else
                    {
                        // Outro skips if we have no completion results
                        Log.Logger.Warning("GAMEPLAY: Finished, cancelled");
                        StopImmediately(false);
                    }
                }
                
                break;
            }
            case GameplayState.Outro:
            {
                var isOutroFinished = _host.SyncTime - _outroStartTime >= OutroTimeMs;
                if (isOutroFinished)
                {
                    Log.Logger.Warning("GAMEPLAY: Finished, outro done");
                    StopImmediately(true);
                }

                break;
            }
        }
    }

    public void RemovePlayer(LobbyPlayer player)
    {
        _playersRemaining.Remove(player);
        _playerSceneSettings.Remove(player.Id);
        _playerSongReady.Remove(player.Id);
    }

    public void HandleGameplayRpc(BaseGameplayRpc gameplayRpc, LobbyPlayer player)
    {
        switch (gameplayRpc)
        {
            case SetGameplaySceneReadyRpc gameplaySceneReadyRpc:
            {
                if (gameplaySceneReadyRpc.PlayerSpecificSettings != null)
                    _playerSceneSettings[player.Id] = gameplaySceneReadyRpc.PlayerSpecificSettings;
                Update();
                break;
            }
            case SetGameplaySongReadyRpc:
            {
                if (!_playerSongReady.Contains(player.Id))
                    _playerSongReady.Add(player.Id);
                Update();
                break;
            }
            case RequestReturnToMenuRpc:
            {
                break;
            }
            case LevelFinishedRpc levelFinishedRpc:
            {
                if (levelFinishedRpc.Results != null)
                    _results[player.Id] = levelFinishedRpc.Results;
                Update();
                break;
            }
        }
    }

    public const long SceneLoadTimeoutMs = 15 * 1000;
    public const long AudioLoadTimeoutMs = 15 * 1000;
    public const long FixedSongStartTimeDelayMs = 250;
    public const long ResultWaitTimeoutMs = 10 * 1000;
    public const long OutroTimeMs = 20 * 1000;

    public enum GameplayState
    {
        NotStarted,
        SceneSyncStart,
        SongSyncStart,
        Gameplay,
        Outro
    }
}