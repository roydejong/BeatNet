// This file was generated by BeatNet.CodeGen (RpcGenerator) - Do not modify manually
using System;
using BeatNet.Lib.BeatSaber.Rpc;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

public sealed class SetIsEntitledToLevelRpc : BaseRpc
{
	public string? LevelId { get; set; } = null;
	public EntitlementsStatus? EntitlementStatus { get; set; } = null;

	public override int ValueCount => 2;
	public override object? Value0 => LevelId;
	public override object? Value1 => EntitlementStatus;
	public override object? Value2 => null;
	public override object? Value3 => null;

	public SetIsEntitledToLevelRpc(string? levelId = null, EntitlementsStatus? entitlementStatus = null)
	{
		LevelId = levelId;
		EntitlementStatus = entitlementStatus;
	}
}
