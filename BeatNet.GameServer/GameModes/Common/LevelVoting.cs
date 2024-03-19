using BeatNet.GameServer.GameModes.Models;
using BeatNet.GameServer.Lobby;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.GameServer.GameModes.Common;

public class LevelVoting
{
    private readonly Dictionary<byte, BeatmapLevel> _recommendedBeatmaps = new();
    private readonly Dictionary<byte, GameplayModifiers> _recommendedModifiers = new();
    
    public event Action<BeatmapLevel?>? TopVotedBeatmapChanged;
    public event Action<GameplayModifiers?>? TopVotedModifiersChanged;
    
    public BeatmapLevel? TopVotedBeatmap { get; private set; }
    public GameplayModifiers? TopVotedModifiers { get; private set; }

    public void Reset()
    {
        _recommendedBeatmaps.Clear();
        _recommendedModifiers.Clear();
        
        UpdateTopVotedBeatmap();
        UpdateTopVotedModifiers();
    }
    
    public void ClearRecommendedBeatmap(LobbyPlayer player)
    {
        _recommendedBeatmaps.Remove(player.Id);
        
        UpdateTopVotedBeatmap();
    }
    
    public void ClearRecommendedModifiers(LobbyPlayer player)
    {
        _recommendedModifiers.Remove(player.Id);
        
        UpdateTopVotedModifiers();
    }

    public void ClearPlayer(LobbyPlayer player)
    {
        ClearRecommendedBeatmap(player);
        ClearRecommendedModifiers(player);
    }
    
    public void SetRecommendedBeatmap(LobbyPlayer player, BeatmapLevel beatmap)
    {
        _recommendedBeatmaps[player.Id] = beatmap;
        
        UpdateTopVotedBeatmap();
    }
    
    public void SetRecommendedModifiers(LobbyPlayer player, GameplayModifiers modifiers)
    {
        _recommendedModifiers[player.Id] = modifiers;
        
        UpdateTopVotedModifiers();
    }
    
    private void UpdateTopVotedBeatmap()
    {
        var topVoted = _recommendedBeatmaps
            .GroupBy(x => x.Value)
            .MaxBy(x => x.Count());

        TopVotedBeatmap = topVoted?.Key;
        TopVotedBeatmapChanged?.Invoke(TopVotedBeatmap);
    }
    
    private void UpdateTopVotedModifiers()
    {
        var topVoted = _recommendedModifiers
            .GroupBy(x => x.Value)
            .MaxBy(x => x.Count());

        TopVotedModifiers = topVoted?.Key;
        TopVotedModifiersChanged?.Invoke(TopVotedModifiers);
    }
}