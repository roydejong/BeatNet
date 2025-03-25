using BeatNet.Lib.MultiplayerCore;
using BeatNet.Lib.Net.IO;
using JetBrains.Annotations;

namespace BeatNet.Lib.MultiplayerChat;

/// <summary>
/// Reliable packet containing a simple text chat message.
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
[UsedImplicitly]
public class MpChatVoicePacket : MpChatBasePacket
{
    public override MpCoreMessageType MpCoreMessageType => MpCoreMessageType.MpChatVoicePacket;
    public override string PacketName => nameof(MpChatVoicePacket);
    
    /// <summary>
    /// Rolling sequence number of the audio fragment (modulo 256).
    /// </summary>
    public uint Index;
    /// <summary>
    /// Opus-encoded audio fragment (48kHz, 1 channel).
    /// If null/empty, this indicates the end of a transmission.
    /// </summary>
    public byte[]? Data;

    public override void WriteTo(ref NetWriter writer)
    {
        base.WriteTo(ref writer);

        writer.WriteVarUInt(Index);
        writer.WriteInt(Data?.Length ?? 0);
        if (Data != null)
            writer.WriteBytes(Data);
    }

    public override void ReadFrom(ref NetReader reader)
    {
        base.ReadFrom(ref reader);
        
        Index = reader.ReadVarUInt();
        
        var dataLength = reader.ReadInt();

        if (dataLength == 0)
        {
            Data = null;
            return;
        }
        
        Data = reader.ReadBytes(dataLength).ToArray(); // TODO Not good, want buffer pool
    }
}