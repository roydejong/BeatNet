using BeatNet.GameServer.BSSB.Utils;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using Newtonsoft.Json;

namespace BeatNet.GameServer.BSSB.Models;

public class BssbServerLevel : JsonObject<BssbServerLevel>
{
    [JsonProperty("LevelId")] public string? LevelId;
    [JsonProperty("SongName")] public string? SongName;
    [JsonProperty("SongSubName")] public string? SongSubName;
    [JsonProperty("SongAuthorName")] public string? SongAuthorName;
    [JsonProperty("CoverUrl")] public string? CoverArtUrl;
    [JsonProperty("SessionGameId")] public string? SessionGameId;
    [JsonProperty("Difficulty")] public BeatmapDifficulty? Difficulty;
    [JsonProperty("Modifiers")] public GameplayModifiers? Modifiers;
    [JsonProperty("Characteristic")] public string? Characteristic;
}