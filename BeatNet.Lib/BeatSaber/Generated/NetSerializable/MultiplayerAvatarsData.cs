// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class MultiplayerAvatarsData : INetSerializable
{
	public List<MultiplayerAvatarData> MultiplayerAvatarsDataValue { get; set; }
	public BitMask128 SupportedAvatarTypeIdHashesBloomFilter { get; set; }

	public MultiplayerAvatarsData(List<MultiplayerAvatarData> multiplayerAvatarsDataValue, BitMask128 supportedAvatarTypeIdHashesBloomFilter)
	{
		MultiplayerAvatarsDataValue = multiplayerAvatarsDataValue;
		SupportedAvatarTypeIdHashesBloomFilter = supportedAvatarTypeIdHashesBloomFilter;
	}

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteSerializableList<List<MultiplayerAvatarData>, MultiplayerAvatarData>(MultiplayerAvatarsDataValue);
		writer.WriteSerializable<BitMask128>(SupportedAvatarTypeIdHashesBloomFilter);
	}

	public void ReadFrom(ref NetReader reader)
	{
		MultiplayerAvatarsDataValue = reader.ReadSerializableList<List<MultiplayerAvatarData>, MultiplayerAvatarData>();
		SupportedAvatarTypeIdHashesBloomFilter = reader.ReadSerializable<BitMask128>();
	}
}