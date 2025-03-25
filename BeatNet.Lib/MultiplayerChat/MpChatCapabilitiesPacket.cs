using BeatNet.Lib.MultiplayerCore;
using BeatNet.Lib.Net.IO;
using JetBrains.Annotations;

namespace BeatNet.Lib.MultiplayerChat;

/// <summary>
/// Reliable packet sent to each player indicating that they have the mod, and which features are supported and enabled.
/// Could be sent as an update when already previously sent.
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
[UsedImplicitly]
public class MpChatCapabilitiesPacket : MpChatBasePacket
{
    public override MpCoreMessageType MpCoreMessageType => MpCoreMessageType.MpChatCapabilitiesPacket;
    public override string PacketName => nameof(MpChatCapabilitiesPacket);
    
    /// <summary>
    /// Is text chat supported and enabled?
    /// </summary>
    public bool CanTextChat;

    /// <summary>
    /// Is voice chat supported and enabled?
    /// </summary>
    public bool CanReceiveVoiceChat;

    /// <summary>
    /// Is voice chat supported and enabled, and is a valid recording device configured?
    /// </summary>
    public bool CanTransmitVoiceChat;

    public override void WriteTo(ref NetWriter writer)
    {
        base.WriteTo(ref writer);

        writer.WriteBool(CanTextChat);
        writer.WriteBool(CanReceiveVoiceChat);
        writer.WriteBool(CanTransmitVoiceChat);
    }

    public override void ReadFrom(ref NetReader reader)
    {
        base.ReadFrom(ref reader);

        CanTextChat = reader.ReadBool();
        CanReceiveVoiceChat = reader.ReadBool();
        CanTransmitVoiceChat = reader.ReadBool();
    }
}