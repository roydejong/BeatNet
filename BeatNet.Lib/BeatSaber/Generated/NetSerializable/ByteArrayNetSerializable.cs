// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class ByteArrayNetSerializable
{
	public byte[] Data { get; set; }
	public int Length { get; set; }
	public string Name { get; set; }
	public bool AllowEmpty { get; set; }
	public int MinLength { get; set; }
	public int MaxLength { get; set; }

	public ByteArrayNetSerializable(byte[] data, int length, string name, bool allowEmpty, int minLength, int maxLength)
	{
		Data = data;
		Length = length;
		Name = name;
		AllowEmpty = allowEmpty;
		MinLength = minLength;
		MaxLength = maxLength;
	}
}