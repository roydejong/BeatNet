// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Rpc;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

public sealed class LevelLoadErrorRpc : BaseRpc
{
	public override byte RpcType => (byte)MenuRpcType.LevelLoadError;

	public string? LevelId { get; set; } = null;

	public override int ValueCount => 1;
	public override object? Value0 => LevelId;
	public override object? Value1 => null;
	public override object? Value2 => null;
	public override object? Value3 => null;

	public LevelLoadErrorRpc(string? levelId = null)
	{
		LevelId = levelId;
	}
}