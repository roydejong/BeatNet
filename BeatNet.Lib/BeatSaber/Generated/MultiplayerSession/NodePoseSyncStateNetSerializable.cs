// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.MultiplayerSession;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class NodePoseSyncStateNetSerializable : BaseSessionPacket
{
	public override SessionMessageType SessionMessageType => SessionMessageType.NodePoseSyncState;

	public SyncStateId Id { get; set; }
	public long Time { get; set; }
	public NodePoseSyncState State { get; set; }

	public NodePoseSyncStateNetSerializable(SyncStateId id, long time, NodePoseSyncState state)
	{
		Id = id;
		Time = time;
		State = state;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		writer.WriteSerializable<SyncStateId>(Id);
		writer.WriteVarULong((ulong)Time);
		writer.WriteSerializable<NodePoseSyncState>(State);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		Id = reader.ReadSerializable<SyncStateId>();
		Time = (long)reader.ReadVarULong();
		State = reader.ReadSerializable<NodePoseSyncState>();
	}
}