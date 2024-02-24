using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Common;

public abstract class BaseMenuRpc : BaseRpc
{
    public abstract MenuRpcType RpcType { get; }
    public override byte RpcTypeValue => (byte)RpcType;
}