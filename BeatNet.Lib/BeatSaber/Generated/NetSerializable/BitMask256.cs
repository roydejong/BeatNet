// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class BitMask256 : INetSerializable
{
	public int BitCount { get; set; }
	public BitMask256 MaxValue { get; set; }
	public ulong D0 { get; set; }
	public ulong D1 { get; set; }
	public ulong D2 { get; set; }
	public ulong D3 { get; set; }

	public BitMask256(int bitCount, BitMask256 maxValue, ulong d0, ulong d1, ulong d2, ulong d3)
	{
		BitCount = bitCount;
		MaxValue = maxValue;
		D0 = d0;
		D1 = d1;
		D2 = d2;
		D3 = d3;
	}

	public void WriteTo(ref NetWriter writer)
	{
		throw new NotImplementedException(); // TODO
	}

	public void ReadFrom(ref NetReader reader)
	{
		throw new NotImplementedException(); // TODO
	}
}