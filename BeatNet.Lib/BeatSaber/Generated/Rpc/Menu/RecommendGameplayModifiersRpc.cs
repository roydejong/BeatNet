// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class RecommendGameplayModifiersRpc : BaseMenuRpc
{
	public override MenuRpcType RpcType => MenuRpcType.RecommendGameplayModifiers;

	public GameplayModifiers? GameplayModifiers { get; set; } = null;

	public RecommendGameplayModifiersRpc(GameplayModifiers? gameplayModifiers = null)
	{
		GameplayModifiers = gameplayModifiers;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		base.WriteTo(ref writer);

		var nullFlags = (byte)(
			(GameplayModifiers != null ? 1 : 0)
		);

		writer.WriteByte(nullFlags);

		if (GameplayModifiers != null)
			writer.WriteSerializable<GameplayModifiers>(GameplayModifiers);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		base.ReadFrom(ref reader);

		var nullFlags = reader.ReadByte();

		if ((nullFlags & (1 << 0)) != 0)
			GameplayModifiers = reader.ReadSerializable<GameplayModifiers>();
		else
			GameplayModifiers = null;
	}
}