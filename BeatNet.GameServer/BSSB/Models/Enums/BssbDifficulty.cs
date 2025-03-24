using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.GameServer.BSSB.Models.Enums;

public enum BssbDifficulty : int
{
    All = -1,
    Easy = 0,
    Normal = 1,
    Hard = 2,
    Expert = 3,
    ExpertPlus = 4,
}

public static class BssbDifficultyUtils
{
    public static BssbDifficulty ToBssbDifficulty(this BeatmapDifficultyMask mask) => mask switch
    {
        BeatmapDifficultyMask.All => BssbDifficulty.All,
        BeatmapDifficultyMask.Easy => BssbDifficulty.Easy,
        BeatmapDifficultyMask.Normal => BssbDifficulty.Normal,
        BeatmapDifficultyMask.Hard => BssbDifficulty.Hard,
        BeatmapDifficultyMask.Expert => BssbDifficulty.Expert,
        BeatmapDifficultyMask.ExpertPlus => BssbDifficulty.ExpertPlus,
        _ => BssbDifficulty.All
    };
        
    public static BssbDifficulty ToBssbDifficulty(this BeatmapDifficulty mask) => mask switch
    {
        BeatmapDifficulty.Easy => BssbDifficulty.Easy,
        BeatmapDifficulty.Normal => BssbDifficulty.Normal,
        BeatmapDifficulty.Hard => BssbDifficulty.Hard,
        BeatmapDifficulty.Expert => BssbDifficulty.Expert,
        BeatmapDifficulty.ExpertPlus => BssbDifficulty.ExpertPlus,
        _ => BssbDifficulty.All
    };
}