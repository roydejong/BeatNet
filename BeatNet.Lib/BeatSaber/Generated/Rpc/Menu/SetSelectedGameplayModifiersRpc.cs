// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Rpc;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class SetSelectedGameplayModifiersRpc : BaseRpc
{
	public override byte RpcType => (byte)MenuRpcType.SetSelectedGameplayModifiers;

	public GameplayModifiers? GameplayModifiers { get; set; } = null;

	public SetSelectedGameplayModifiersRpc(GameplayModifiers? gameplayModifiers = null)
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