// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class BitMask128
{
	public int BitCount { get; set; }
	public BitMask128 MaxValue { get; set; }
	public ulong D0 { get; set; }
	public ulong D1 { get; set; }

	public BitMask128(int bitCount, BitMask128 maxValue, ulong d0, ulong d1)
	{
		BitCount = bitCount;
		MaxValue = maxValue;
		D0 = d0;
		D1 = d1;
	}
}