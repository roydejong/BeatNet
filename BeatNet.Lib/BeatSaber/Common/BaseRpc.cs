using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Common;

public abstract class BaseRpc : BaseSessionPacket
{
    public abstract byte RpcTypeValue { get; }
    
    public long SyncTime { get; set; }

    public override void WriteTo(ref NetWriter writer)
    {
        writer.WriteVarULong((ulong)SyncTime);
    }

    public override void ReadFrom(ref NetReader reader)
    {
        SyncTime = (long)reader.ReadVarULong();
    }
}