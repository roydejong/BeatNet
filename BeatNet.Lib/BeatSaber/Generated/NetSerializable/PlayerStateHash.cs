// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class PlayerStateHash
{
	public BitMask128 BloomFilter { get; set; }
	public string KToStringPrefix { get; set; }
	public string KToStringSuffix { get; set; }

	public PlayerStateHash(BitMask128 bloomFilter, string kToStringPrefix, string kToStringSuffix)
	{
		BloomFilter = bloomFilter;
		KToStringPrefix = kToStringPrefix;
		KToStringSuffix = kToStringSuffix;
	}
}