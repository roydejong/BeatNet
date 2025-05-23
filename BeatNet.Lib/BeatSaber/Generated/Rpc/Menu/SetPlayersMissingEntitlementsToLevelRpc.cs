// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class SetPlayersMissingEntitlementsToLevelRpc : BaseMenuRpc
{
	public override MenuRpcType RpcType => MenuRpcType.SetPlayersMissingEntitlementsToLevel;

	public PlayersMissingEntitlementsNetSerializable? PlayersMissingEntitlements { get; set; } = null;

	public SetPlayersMissingEntitlementsToLevelRpc(PlayersMissingEntitlementsNetSerializable? playersMissingEntitlements = null)
	{
		PlayersMissingEntitlements = playersMissingEntitlements;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		base.WriteTo(ref writer);

		var nullFlags = (byte)(
			(PlayersMissingEntitlements != null ? 1 : 0)
		);

		writer.WriteByte(nullFlags);

		if (PlayersMissingEntitlements != null)
			writer.WriteSerializable<PlayersMissingEntitlementsNetSerializable>(PlayersMissingEntitlements);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		base.ReadFrom(ref reader);

		var nullFlags = reader.ReadByte();

		if ((nullFlags & (1 << 0)) != 0)
			PlayersMissingEntitlements = reader.ReadSerializable<PlayersMissingEntitlementsNetSerializable>();
		else
			PlayersMissingEntitlements = null;
	}
}