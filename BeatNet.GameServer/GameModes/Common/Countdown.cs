using BeatNet.GameServer.GameModes.Models;
using BeatNet.GameServer.Lobby;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.GameServer.GameModes.Common;

public class Countdown
{
    public const int LockInTimeMs = 5 * 1000;
    public const int AllReadyTimeMs = 5 * 1000;
    public const int SomeReadyTimeMs = 10 * 1000;
    
    private readonly LobbyHost _host;
    private BeatmapLevel? _level;
    private GameplayModifiers? _modifiers;
    private readonly Dictionary<byte, bool> _readyPlayers;
    
    public bool IsCountingDown { get; private set; }
    public bool IsLockedIn { get; private set; }
    public long? CountdownEndTime { get; private set; }

    public event Action<long>? CountdownEndTimeSet; 
    public event Action<BeatmapLevel, GameplayModifiers?, long>? CountdownLockedIn;
    public event Action? CountdownFinished;
    public event Action? CountdownCancelled;

    public Countdown(LobbyHost host)
    {
        _host = host;
        _level = null;
        _modifiers = null;
        _readyPlayers = new();
    }

    public void Reset()
    {
        _level = null;
        _modifiers = null;
        _readyPlayers.Clear();
        
        IsCountingDown = false;
        IsLockedIn = false;
        CountdownEndTime = null;
        
        CountdownCancelled?.Invoke();
    }

    public void SetSelectedLevel(BeatmapLevel? level, GameplayModifiers? modifiers)
    {
        _level = level;
        _modifiers = modifiers;
        
        Update();
    }

    public void SetPlayerReady(LobbyPlayer player, bool ready)
    {
        _readyPlayers[player.Id] = ready;
        
        Update();
    }

    public void Update()
    {
        var hasLevelSelected = _level != null;
        var playerCount = _host.ConnectedPlayers.Count;
        var anyPlayersReady = _readyPlayers.Values.Any(x => x);
        var allPlayersReady = _readyPlayers.Values.Count(x => x) == playerCount;

        if (!hasLevelSelected || !anyPlayersReady)
        {
            if (IsCountingDown)
                // Cancel countdown - no level selected and/or no players ready
                Reset();
            return;
        }
        
        var maxCountdownTime = (playerCount > 1 && allPlayersReady) ? AllReadyTimeMs : SomeReadyTimeMs;
        
        if (!IsCountingDown)
        {
            // Start countdown
            IsCountingDown = true;
            CountdownEndTime = _host.SyncTime + maxCountdownTime;
            CountdownEndTimeSet?.Invoke(CountdownEndTime!.Value);
            return;
        }
        
        var remainingTime = CountdownEndTime!.Value - _host.SyncTime;
        
        if (!IsLockedIn && remainingTime > maxCountdownTime)
        {
            // We can reduce the countdown time (when all players are ready)
            CountdownEndTime = _host.SyncTime + maxCountdownTime;
            CountdownEndTimeSet?.Invoke(CountdownEndTime!.Value);
        }
        
        if (remainingTime <= LockInTimeMs && !IsLockedIn)
        {
            // Lock in level and modifiers
            IsLockedIn = true;
            CountdownLockedIn?.Invoke(_level!, _modifiers, CountdownEndTime.Value);
            return;
        }

        if (remainingTime <= 0)
        {
            // Countdown finished
            CountdownFinished?.Invoke();
            Reset();
            return;
        }
    }
}