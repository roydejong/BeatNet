using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Common;

public abstract class BaseGameplayRpc : BaseRpc
{
    public override SessionMessageType SessionMessageType => SessionMessageType.GameplayRpc;
    public abstract GameplayRpcType RpcType { get; }
    public override byte RpcTypeValue => (byte)RpcType;
}