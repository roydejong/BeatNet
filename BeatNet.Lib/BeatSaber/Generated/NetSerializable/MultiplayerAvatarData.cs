// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class MultiplayerAvatarData : INetSerializable
{
	public uint AvatarTypeIdentifierHash { get; set; }
	public byte[] Data { get; set; }

	public MultiplayerAvatarData(uint avatarTypeIdentifierHash, byte[] data)
	{
		AvatarTypeIdentifierHash = avatarTypeIdentifierHash;
		Data = data;
	}

	public void WriteTo(ref NetWriter writer)
	{
		// MultiplayerAvatarDataFixedImpl
		writer.WriteUInt(AvatarTypeIdentifierHash);
		writer.WriteByteArray(Data);
	}

	public void ReadFrom(ref NetReader reader)
	{
		// MultiplayerAvatarDataFixedImpl
		AvatarTypeIdentifierHash = reader.ReadUInt();
		Data = reader.ReadByteArray();
	}
}