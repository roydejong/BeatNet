using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.MultiplayerCore;

public abstract class BaseMpcPacket : BaseSessionPacket
{
    public override SessionMessageType SessionMessageType => SessionMessageType.MultiplayerCore;
    public abstract MpcMessageType MpcMessageType { get; }

    public abstract string PacketName { get; }
}