using BeatNet.GameServer.BSSB.Utils;
using Newtonsoft.Json;

namespace BeatNet.GameServer.BSSB.Models;

public class UnAnnounceParams : JsonObject<UnAnnounceParams>
{
    [JsonProperty("SelfUserId")] public string? SelfUserId;
    [JsonProperty("HostUserId")] public string? HostUserId;
    [JsonProperty("HostSecret")] public string? HostSecret;
}