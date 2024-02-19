// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class MultiplayerAvatarsData
{
	public List<MultiplayerAvatarData> MultiplayerAvatarsData { get; set; }
	public BitMask128 SupportedAvatarTypeIdHashesBloomFilter { get; set; }

	public MultiplayerAvatarsData(List<MultiplayerAvatarData> multiplayerAvatarsData, BitMask128 supportedAvatarTypeIdHashesBloomFilter)
	{
		MultiplayerAvatarsData = multiplayerAvatarsData;
		SupportedAvatarTypeIdHashesBloomFilter = supportedAvatarTypeIdHashesBloomFilter;
	}
}
