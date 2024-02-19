// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Rpc;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

public sealed class SetCountdownEndTimeRpc : BaseRpc
{
	public override byte RpcType => (byte)MenuRpcType.SetCountdownEndTime;

	public long? NewTime { get; set; } = null;

	public override int ValueCount => 1;
	public override object? Value0 => NewTime;
	public override object? Value1 => null;
	public override object? Value2 => null;
	public override object? Value3 => null;

	public SetCountdownEndTimeRpc(long? newTime = null)
	{
		NewTime = newTime;
	}
}