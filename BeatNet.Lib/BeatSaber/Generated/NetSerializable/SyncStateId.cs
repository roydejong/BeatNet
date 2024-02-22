// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class SyncStateId : INetSerializable
{
	public const byte kMaxValue = 128;

	public byte Id { get; set; }
	public bool Flag { get; set; }

	public SyncStateId(byte id, bool flag)
	{
		Id = id;
		Flag = flag;
	}

	public void WriteTo(ref NetWriter writer)
	{
		// SyncStateIdFixedImpl
		writer.WriteByte((byte)(Id | (Flag ? 128 : 0)));
	}

	public void ReadFrom(ref NetReader reader)
	{
		// SyncStateIdFixedImpl
		var value = reader.ReadByte();
		Id = (byte)(value & 127);
		Flag = (value & 128) != 0;
	}
}