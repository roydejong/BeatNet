using System.Net;
using BeatNet.GameServer.BSSB.Models.Enums;
using BeatNet.GameServer.BSSB.Utils;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using Newtonsoft.Json;

namespace BeatNet.GameServer.BSSB.Models;

public class BssbServer : JsonObject<BssbServer>
{
    [JsonProperty("OwnerId")] public string? RemoteUserId;
    [JsonProperty("OwnerName")] public string? RemoteUserName;
    [JsonProperty("HostSecret")] public string? HostSecret;
    [JsonProperty("ManagerId")] public string? ManagerId;
    [JsonProperty("PlayerCount")] public int? PlayerCount;
    [JsonProperty("PlayerLimit")] public int? PlayerLimit;
    [JsonProperty("GameplayMode")] public GameplayServerMode? GameplayMode;
    [JsonProperty("GameName")] public string? Name;
    [JsonProperty("LobbyState")] public MultiplayerLobbyState? LobbyState;
    [JsonProperty("Level")] public BssbServerLevel? Level;
    [JsonProperty("Difficulty")] public BssbDifficulty? LobbyDifficulty;
    [JsonProperty("LevelDifficulty")] public BssbDifficulty? LevelDifficulty;
    [JsonProperty("ServerType")] public string? ServerTypeCode;
    [JsonProperty("Endpoint")] public DnsEndPoint? EndPoint;
    [JsonProperty("GameVersion")] public string? GameVersion;
    [JsonProperty("MpCoreVersion")] public string? MultiplayerCoreVersion;
    [JsonProperty("MpExVersion")] public string? MultiplayerExtensionsVersion;
    [JsonProperty("ServerTypeText")] public string? ServerTypeText;
    [JsonProperty("EncryptionMode")] public string? EncryptionMode;
    [JsonProperty("Players")] public List<BssbServerPlayer> Players = new();
}