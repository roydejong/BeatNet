using BeatNet.GameServer.BSSB.Utils;
using Newtonsoft.Json;

namespace BeatNet.GameServer.BSSB.Models;

public class BssbServerPlayer : JsonObject<BssbServerPlayer>
{
    [JsonProperty("UserId")] public string? UserId;
    [JsonProperty("UserName")] public string? UserName;
    [JsonProperty("PlatformType")] public string? PlatformType;
    [JsonProperty("PlatformUserId")] public string? PlatformUserId;
    [JsonProperty("SortIndex")] public int SortIndex;
    [JsonProperty("IsMe")] public bool IsMe;
    [JsonProperty("IsHost")] public bool IsHost;
    [JsonProperty("IsPartyLeader")] public bool IsPartyLeader;
    [JsonProperty("IsAnnouncer")] public bool IsAnnouncing;
    [JsonProperty("Latency")] public float CurrentLatency;
}