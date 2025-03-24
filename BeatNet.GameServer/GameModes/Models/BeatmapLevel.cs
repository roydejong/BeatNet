using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.MultiplayerCore;

namespace BeatNet.GameServer.GameModes.Models;

public class BeatmapLevel
{
    public string LevelId { get; private init; } = null!;
    public string Characteristic { get; private init; } = null!;
    public BeatmapDifficulty Difficulty { get; private init; }
    public string? SongName { get; set; }
    public string? SongSubName { get; set; }
    public string? SongAuthorName { get; set; }

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

    public static BeatmapLevel FromMpBeatmapPacket(MpBeatmapPacket packet)
    {
        return new BeatmapLevel()
        {
            LevelId = $"custom_level_{packet.LevelHash}",
            Characteristic = packet.CharacteristicName,
            Difficulty = packet.Difficulty,
            SongName = packet.SongName,
            SongAuthorName = packet.SongAuthorName,
            SongSubName = packet.SongSubName
        };
    }

    public BeatmapKeyNetSerializable ToBeatmapKey() =>
        new(LevelId, Characteristic, Difficulty);
}