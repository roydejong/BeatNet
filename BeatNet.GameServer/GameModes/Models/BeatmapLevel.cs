using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.GameServer.GameModes.Models;

public class BeatmapLevel
{
    public string LevelId { get; private init; } = null!;
    public string Characteristic { get; private init; } = null!;
    public BeatmapDifficulty Difficulty { get; private init; }

    public bool IsCustomLevel => LevelId.StartsWith("custom_level_");

    private BeatmapLevel()
    {
    }

    public static BeatmapLevel FromBeatmapKey(BeatmapKeyNetSerializable key)
    {
        return new BeatmapLevel
        {
            LevelId = key.LevelID,
            Characteristic = key.BeatmapCharacteristicSerializedName,
            Difficulty = key.Difficulty
        };
    }

    public BeatmapKeyNetSerializable ToBeatmapKey() =>
        new(LevelId, Characteristic, Difficulty);
}