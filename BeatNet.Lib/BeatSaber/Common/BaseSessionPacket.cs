using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Common;

public abstract class BaseSessionPacket : BaseCpmPacket
{
    public override InternalMessageType InternalMessageType => InternalMessageType.MultiplayerSession;
    public abstract SessionMessageType SessionMessageType { get; }
}