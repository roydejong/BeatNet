using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Rpc;

public abstract class BaseRpc : INetSerializable
{
    public abstract byte RpcType { get; }
    
    public long SyncTime { get; set; }

    public virtual void WriteTo(ref NetWriter writer)
    {
        writer.WriteVarULong((ulong)SyncTime);
    }

    public virtual void ReadFrom(ref NetReader reader)
    {
        SyncTime = (long)reader.ReadVarULong();
    }
}