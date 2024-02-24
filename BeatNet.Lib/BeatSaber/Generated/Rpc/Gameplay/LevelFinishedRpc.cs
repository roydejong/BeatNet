// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Gameplay;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class LevelFinishedRpc : BaseGameplayRpc
{
	public override GameplayRpcType RpcType => GameplayRpcType.LevelFinished;

	public MultiplayerLevelCompletionResults? Results { get; set; } = null;

	public LevelFinishedRpc(MultiplayerLevelCompletionResults? results = null)
	{
		Results = results;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		base.WriteTo(ref writer);

		var nullFlags = (byte)(
			(Results != null ? 1 : 0)
		);

		writer.WriteByte(nullFlags);

		if (Results != null)
			writer.WriteSerializable<MultiplayerLevelCompletionResults>(Results);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		base.ReadFrom(ref reader);

		var nullFlags = reader.ReadByte();

		if ((nullFlags & (1 << 0)) != 0)
			Results = reader.ReadSerializable<MultiplayerLevelCompletionResults>();
		else
			Results = null;
	}
}