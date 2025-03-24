using BeatNet.GameServer.BSSB.Utils;
using JetBrains.Annotations;

namespace BeatNet.GameServer.BSSB.Models;

[UsedImplicitly]
public class UnAnnounceResponse : JsonObject<UnAnnounceResponse>
{
    public string? Result;
    public bool CanRetry;
}