// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.BeatSaber.Rpc;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class GetRecommendedGameplayModifiersRpc : BaseRpc
{
	public override byte RpcType => (byte)MenuRpcType.GetRecommendedGameplayModifiers;

	public GetRecommendedGameplayModifiersRpc()
	{
		// RPC without parameters
	}
}