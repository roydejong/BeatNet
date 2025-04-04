// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class SetIsEntitledToLevelRpc : BaseMenuRpc
{
	public override MenuRpcType RpcType => MenuRpcType.SetIsEntitledToLevel;

	public string? LevelId { get; set; } = null;
	public EntitlementsStatus? EntitlementStatus { get; set; } = null;

	public SetIsEntitledToLevelRpc(string? levelId = null, EntitlementsStatus? entitlementStatus = null)
	{
		LevelId = levelId;
		EntitlementStatus = entitlementStatus;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		base.WriteTo(ref writer);

		var nullFlags = (byte)(
			(LevelId != null ? 1 : 0) | 
			(EntitlementStatus != null ? 2 : 0)
		);

		writer.WriteByte(nullFlags);

		if (LevelId != null)
			writer.WriteString(LevelId);

		if (EntitlementStatus != null)
			writer.WriteEnum<EntitlementsStatus>(EntitlementStatus.Value);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		base.ReadFrom(ref reader);

		var nullFlags = reader.ReadByte();

		if ((nullFlags & (1 << 0)) != 0)
			LevelId = reader.ReadString();
		else
			LevelId = null;

		if ((nullFlags & (1 << 1)) != 0)
			EntitlementStatus = reader.ReadEnum<EntitlementsStatus>();
		else
			EntitlementStatus = null;
	}
}