using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Common;

public abstract class BaseCpmPacket : INetSerializable
{
    public abstract InternalMessageType InternalMessageType { get; }
    
    public abstract void WriteTo(ref NetWriter writer);
    public abstract void ReadFrom(ref NetReader reader);
}