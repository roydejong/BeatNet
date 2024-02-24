using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Common;

public abstract class BaseRpc : INetSerializable
{
    public abstract byte RpcTypeValue { get; }
    
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