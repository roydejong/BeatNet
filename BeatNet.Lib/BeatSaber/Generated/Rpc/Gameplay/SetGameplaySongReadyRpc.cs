// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.BeatSaber.Rpc;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Gameplay;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class SetGameplaySongReadyRpc : BaseRpc
{
	public override byte RpcType => (byte)GameplayRpcType.SetGameplaySongReady;

	public SetGameplaySongReadyRpc()
	{
		// RPC without parameters
	}
}