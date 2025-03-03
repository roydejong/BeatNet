using BeatNet.Lib.MultiplayerCore.Enums;
using BeatNet.Lib.Net.IO;
using JetBrains.Annotations;

namespace BeatNet.Lib.MultiplayerCore;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
[UsedImplicitly]
public class MpPlayerDataPacket : BaseMpCorePacket
{
    public override MpCoreMessageType MpCoreMessageType => MpCoreMessageType.MpPlayerData;
    public override string PacketName => "MpPlayerData";

    public string PlatformUserId { get; set; } = null!;
    public MpCorePlatform Platform { get; set; } 
    public string GameVersion { get; set; } = null!;
    
    public override void WriteTo(ref NetWriter writer)
    {
        writer.WriteString(PlatformUserId);
        writer.WriteInt((int)Platform);
        writer.WriteString(GameVersion);
    }

    public override void ReadFrom(ref NetReader reader)
    {
        PlatformUserId = reader.ReadString();
        Platform = reader.ReadIntEnum<MpCorePlatform>();
        GameVersion = reader.ReadString();
    }
}