// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class InvalidateLevelEntitlementStatusesRpc : BaseMenuRpc
{
	public override MenuRpcType RpcType => MenuRpcType.InvalidateLevelEntitlementStatuses;

	public InvalidateLevelEntitlementStatusesRpc()
	{
		// RPC without parameters
	}

	public static readonly InvalidateLevelEntitlementStatusesRpc Instance = new();
}