using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.MultiplayerCore;

public class GenericMpCorePacket : BaseMpCorePacket
{
    public override MpCoreMessageType MpCoreMessageType => MpCoreMessageType.Generic;

    public string PacketNameValue { get; set; } = "";
    public override string PacketName => PacketNameValue;
    
    public byte[]? Payload;
    
    public override void WriteTo(ref NetWriter writer)
    {
        if (Payload != null)
            writer.WriteBytes(Payload);
    }

    public override void ReadFrom(ref NetReader reader)
    {
        if (!reader.EndOfData)
            Payload = reader.ReadBytes(reader.RemainingLength).ToArray(); // TODO Not ideal
        else
            Payload = null;
    }
}