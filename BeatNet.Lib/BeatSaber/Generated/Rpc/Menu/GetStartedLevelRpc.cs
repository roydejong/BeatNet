// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Rpc;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

public sealed class GetStartedLevelRpc : BaseSimpleRpc
{
	public override byte RpcType => (byte)MenuRpcType.GetStartedLevel;

	public GetStartedLevelRpc()
	{
		// RPC without parameters
	}
}