using BeatNet.GameServer.BSSB.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatNet.GameServer.BSSB.Models;

[UsedImplicitly]
public class AnnounceResponse : JsonObject<AnnounceResponse>
{
    [JsonProperty("Success")] public bool Success;
    [JsonProperty("Key")] public string? Key;
    [JsonProperty("Message")] public string? ServerMessage;
}