// This file was generated by BeatNet.CodeGen (PacketGenerator)
// Do not modify manually

using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Packet;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class PlayerConnectedPacket : INetSerializable
{
	public byte RemoteConnectionId { get; set; }
	public string UserId { get; set; }
	public string UserName { get; set; }
	public bool IsConnectionOwner { get; set; }

	public PlayerConnectedPacket(byte remoteConnectionId, string userId, string userName, bool isConnectionOwner)
	{
		RemoteConnectionId = remoteConnectionId;
		UserId = userId;
		UserName = userName;
		IsConnectionOwner = isConnectionOwner;
	}

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteByte(RemoteConnectionId);
		writer.WriteString(UserId);
		writer.WriteString(UserName);
		writer.WriteBool(IsConnectionOwner);
	}

	public void ReadFrom(ref NetReader reader)
	{
		RemoteConnectionId = reader.ReadByte();
		UserId = reader.ReadString();
		UserName = reader.ReadString();
		IsConnectionOwner = reader.ReadBool();
	}
}