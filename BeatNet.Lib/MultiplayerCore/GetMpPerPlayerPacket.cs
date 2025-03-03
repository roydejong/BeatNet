using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.MultiplayerCore.Serializable;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.SongCore;
using JetBrains.Annotations;

namespace BeatNet.Lib.MultiplayerCore;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
[UsedImplicitly]
public class GetMpPerPlayerPacket : BaseMpCorePacket
{
    public override MpCoreMessageType MpCoreMessageType => MpCoreMessageType.GetMpPerPlayerPacket;
    public override string PacketName => "GetMpPerPlayerPacket";

    public override void WriteTo(ref NetWriter writer)
    {
        // Request with empty payload
    }

    public override void ReadFrom(ref NetReader reader)
    {
        // Request with empty payload
    }
}