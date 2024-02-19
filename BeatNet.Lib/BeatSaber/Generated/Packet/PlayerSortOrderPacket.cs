// This file was generated by BeatNet.CodeGen (PacketGenerator)
// Do not modify manually

using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Packet;

public sealed class PlayerSortOrderPacket : INetSerializable
{
	public string UserId { get; set; }
	public int SortIndex { get; set; }

	public PlayerSortOrderPacket(string userId, int sortIndex)
	{
		UserId = userId;
		SortIndex = sortIndex;
	}

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteString(UserId);
		writer.WriteVarInt(SortIndex);
	}

	public void ReadFrom(ref NetReader reader)
	{
		UserId = reader.ReadString();
		SortIndex = reader.ReadVarInt();
	}
}