// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class ColorNoAlphaSerializable : INetSerializable
{
	public float R { get; set; }
	public float G { get; set; }
	public float B { get; set; }
	public float A { get; set; }

	public ColorNoAlphaSerializable(float r, float g, float b, float a)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteFloat(R);
		writer.WriteFloat(G);
		writer.WriteFloat(B);
		writer.WriteFloat(1f);
	}

	public void ReadFrom(ref NetReader reader)
	{
		R = reader.ReadFloat();
		G = reader.ReadFloat();
		B = reader.ReadFloat();
		A = 1f;
	}
}