// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class Vector3Serializable
{
	public int X { get; set; }
	public int Y { get; set; }
	public int Z { get; set; }

	public Vector3Serializable(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}
}