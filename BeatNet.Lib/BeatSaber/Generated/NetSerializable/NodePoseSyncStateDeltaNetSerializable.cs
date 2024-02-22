// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class NodePoseSyncStateDeltaNetSerializable : INetSerializable
{
	public SyncStateId BaseId { get; set; }
	public int TimeOffsetMs { get; set; }
	public NodePoseSyncState Delta { get; set; }

	public NodePoseSyncStateDeltaNetSerializable(SyncStateId baseId, int timeOffsetMs, NodePoseSyncState delta)
	{
		BaseId = baseId;
		TimeOffsetMs = timeOffsetMs;
		Delta = delta;
	}

	public void WriteTo(ref NetWriter writer)
	{
		// SyncStateDeltaFixedImpl
		writer.WriteSerializable(BaseId);
		writer.WriteInt(TimeOffsetMs);
		if (!BaseId.Flag)
			writer.WriteSerializable(Delta);
	}

	public void ReadFrom(ref NetReader reader)
	{
		// SyncStateDeltaFixedImpl
		BaseId = reader.ReadSerializable<SyncStateId>();
		TimeOffsetMs = reader.ReadInt();
		if (!BaseId.Flag)
			Delta = reader.ReadSerializable<NodePoseSyncState>();
	}
}