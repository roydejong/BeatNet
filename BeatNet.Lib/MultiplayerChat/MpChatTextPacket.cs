using BeatNet.Lib.MultiplayerCore;
using BeatNet.Lib.Net.IO;
using JetBrains.Annotations;

namespace BeatNet.Lib.MultiplayerChat;

/// <summary>
/// Reliable packet containing a simple text chat message.
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
[UsedImplicitly]
public class MpChatTextPacket : MpChatBasePacket
{
    public override MpCoreMessageType MpCoreMessageType => MpCoreMessageType.MpChatTextPacket;
    public override string PacketName => nameof(MpChatTextPacket);
    
    /// <summary>
    /// Raw chat message.
    /// Note: any HTML-style `<tags>` will be stripped from the message before it is displayed, to avoid rich text chaos.
    /// </summary>
    public string? Text;

    public override void WriteTo(ref NetWriter writer)
    {
        base.WriteTo(ref writer);

        writer.WriteString(Text);
    }

    public override void ReadFrom(ref NetReader reader)
    {
        base.ReadFrom(ref reader);

        Text = reader.ReadString();
    }
}