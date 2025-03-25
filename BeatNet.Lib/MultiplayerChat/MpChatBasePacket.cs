using BeatNet.Lib.MultiplayerCore;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.MultiplayerChat;

// ReSharper disable MemberCanBePrivate.Global
public abstract class MpChatBasePacket : BaseMpCorePacket
{
    public const int MinProtocolVersion = 2;
    public const int MaxProtocolVersion = 2;
    
    /// <summary>
    /// The MPChat protocol version used by the client.
    /// Automatically set for outgoing packets.
    /// </summary>
    public uint ProtocolVersion;

    public override void WriteTo(ref NetWriter writer)
    {
        if (ProtocolVersion <= 0)
            ProtocolVersion = MaxProtocolVersion;
        
        writer.WriteVarUInt(ProtocolVersion);
    }

    public override void ReadFrom(ref NetReader reader)
    {
        ProtocolVersion = reader.ReadVarUInt();
        
        if (ProtocolVersion is < MinProtocolVersion or > MaxProtocolVersion)
            throw new InvalidDataException($"Unsupported protocol version for MpChat: {ProtocolVersion}");
    }
}