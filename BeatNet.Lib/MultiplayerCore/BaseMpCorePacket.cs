using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.MultiplayerCore;

public abstract class BaseMpCorePacket : BaseSessionPacket
{
    public override SessionMessageType SessionMessageType => SessionMessageType.MultiplayerCore;
    public abstract MpCoreMessageType MpCoreMessageType { get; }

    public abstract string PacketName { get; }
}