// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class Vector4Serializable : INetSerializable
{
	public int X { get; set; }
	public int Y { get; set; }
	public int Z { get; set; }
	public int W { get; set; }

	public Vector4Serializable(int x, int y, int z, int w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
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