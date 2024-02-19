// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class BeatmapLevelMask
{
	public int KBitCount { get; set; }
	public int KHashCount { get; set; }
	public int KHashBits { get; set; }
	public BitMaskSparse BloomFilter { get; set; }
	public string KToStringPrefix { get; set; }
	public string KToStringSuffix { get; set; }

	public BeatmapLevelMask(int kBitCount, int kHashCount, int kHashBits, BitMaskSparse bloomFilter, string kToStringPrefix, string kToStringSuffix)
	{
		KBitCount = kBitCount;
		KHashCount = kHashCount;
		KHashBits = kHashBits;
		BloomFilter = bloomFilter;
		KToStringPrefix = kToStringPrefix;
		KToStringSuffix = kToStringSuffix;
	}
}