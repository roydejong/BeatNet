// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class SetCountdownEndTimeRpc : BaseMenuRpc
{
	public override MenuRpcType RpcType => MenuRpcType.SetCountdownEndTime;

	public long? NewTime { get; set; } = null;

	public SetCountdownEndTimeRpc(long? newTime = null)
	{
		NewTime = newTime;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		base.WriteTo(ref writer);

		var nullFlags = (byte)(
			(NewTime != null ? 1 : 0)
		);

		writer.WriteByte(nullFlags);

		if (NewTime != null)
			writer.WriteVarLong(NewTime.Value);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		base.ReadFrom(ref reader);

		var nullFlags = reader.ReadByte();

		if ((nullFlags & (1 << 0)) != 0)
			NewTime = reader.ReadVarLong();
		else
			NewTime = null;
	}
}