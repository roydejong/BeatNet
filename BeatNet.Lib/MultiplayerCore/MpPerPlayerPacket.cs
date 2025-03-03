using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.MultiplayerCore.Serializable;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.SongCore;
using JetBrains.Annotations;

namespace BeatNet.Lib.MultiplayerCore;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
[UsedImplicitly]
public class MpPerPlayerPacket : BaseMpCorePacket
{
    public override MpCoreMessageType MpCoreMessageType => MpCoreMessageType.MpPerPlayerPacket;
    public override string PacketName => "MpPerPlayerPacket";

    /// <summary>
    /// "Per player difficulty" enabled
    /// </summary>
    public bool PPDEnabled { get; set; }
    /// <summary>
    /// "Per player modifiers" enabled
    /// </summary>
    public bool PPMEnabled { get; set; }

    public override void WriteTo(ref NetWriter writer)
    {
        writer.WriteBool(PPDEnabled);
        writer.WriteBool(PPMEnabled);
    }

    public override void ReadFrom(ref NetReader reader)
    {
        PPDEnabled = reader.ReadBool();
        PPMEnabled = reader.ReadBool();
    }
}