// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Rpc;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Menu;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class SetIsStartButtonEnabledRpc : BaseRpc
{
	public override byte RpcType => (byte)MenuRpcType.SetIsStartButtonEnabled;

	public CannotStartGameReason? Reason { get; set; } = null;

	public SetIsStartButtonEnabledRpc(CannotStartGameReason? reason = null)
	{
		Reason = reason;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		base.WriteTo(ref writer);

		var nullFlags = (byte)(
			(Reason != null ? 1 : 0)
		);

		writer.WriteByte(nullFlags);

		if (Reason != null)
			writer.WriteEnum<CannotStartGameReason>(Reason.Value);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		base.ReadFrom(ref reader);

		var nullFlags = reader.ReadByte();

		if ((nullFlags & (1 << 0)) != 0)
			Reason = reader.ReadEnum<CannotStartGameReason>();
		else
			Reason = null;
	}
}