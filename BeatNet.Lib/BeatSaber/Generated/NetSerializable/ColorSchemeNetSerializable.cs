// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class ColorSchemeNetSerializable : INetSerializable
{
	public ColorNoAlphaSerializable SaberAColor { get; set; }
	public ColorNoAlphaSerializable SaberBColor { get; set; }
	public ColorNoAlphaSerializable ObstaclesColor { get; set; }
	public ColorNoAlphaSerializable EnvironmentColor0 { get; set; }
	public ColorNoAlphaSerializable EnvironmentColor1 { get; set; }
	public ColorNoAlphaSerializable EnvironmentColor0Boost { get; set; }
	public ColorNoAlphaSerializable EnvironmentColor1Boost { get; set; }

	public ColorSchemeNetSerializable(ColorNoAlphaSerializable saberAColor, ColorNoAlphaSerializable saberBColor, ColorNoAlphaSerializable obstaclesColor, ColorNoAlphaSerializable environmentColor0, ColorNoAlphaSerializable environmentColor1, ColorNoAlphaSerializable environmentColor0Boost, ColorNoAlphaSerializable environmentColor1Boost)
	{
		SaberAColor = saberAColor;
		SaberBColor = saberBColor;
		ObstaclesColor = obstaclesColor;
		EnvironmentColor0 = environmentColor0;
		EnvironmentColor1 = environmentColor1;
		EnvironmentColor0Boost = environmentColor0Boost;
		EnvironmentColor1Boost = environmentColor1Boost;
	}

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteSerializable<ColorNoAlphaSerializable>(SaberAColor);
		writer.WriteSerializable<ColorNoAlphaSerializable>(SaberBColor);
		writer.WriteSerializable<ColorNoAlphaSerializable>(ObstaclesColor);
		writer.WriteSerializable<ColorNoAlphaSerializable>(EnvironmentColor0);
		writer.WriteSerializable<ColorNoAlphaSerializable>(EnvironmentColor1);
		writer.WriteSerializable<ColorNoAlphaSerializable>(EnvironmentColor0Boost);
		writer.WriteSerializable<ColorNoAlphaSerializable>(EnvironmentColor1Boost);
	}

	public void ReadFrom(ref NetReader reader)
	{
		SaberAColor = reader.ReadSerializable<ColorNoAlphaSerializable>();
		SaberBColor = reader.ReadSerializable<ColorNoAlphaSerializable>();
		ObstaclesColor = reader.ReadSerializable<ColorNoAlphaSerializable>();
		EnvironmentColor0 = reader.ReadSerializable<ColorNoAlphaSerializable>();
		EnvironmentColor1 = reader.ReadSerializable<ColorNoAlphaSerializable>();
		EnvironmentColor0Boost = reader.ReadSerializable<ColorNoAlphaSerializable>();
		EnvironmentColor1Boost = reader.ReadSerializable<ColorNoAlphaSerializable>();
	}
}