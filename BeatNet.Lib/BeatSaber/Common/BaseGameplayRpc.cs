using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Common;

public abstract class BaseGameplayRpc : BaseRpc
{
    public abstract GameplayRpcType RpcType { get; }
    public override byte RpcTypeValue => (byte)RpcType;
}