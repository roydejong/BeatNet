// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class BeatmapLevelMask : INetSerializable
{
	public const int kBitCount = 16384;
	public const int kHashCount = 1;
	public const int kHashBits = 14;
	public const string kToStringPrefix = "[BeatmapLevelMask ";
	public const string kToStringSuffix = "]";

	public BitMaskSparse BloomFilter { get; set; }

	public BeatmapLevelMask(BitMaskSparse? bloomFilter = null)
	{
		BloomFilter = bloomFilter ?? new BitMaskSparse(16384);
	}

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteSerializable<BitMaskSparse>(BloomFilter);
	}

	public void ReadFrom(ref NetReader reader)
	{
		BloomFilter = reader.ReadSerializable<BitMaskSparse>();
	}
}