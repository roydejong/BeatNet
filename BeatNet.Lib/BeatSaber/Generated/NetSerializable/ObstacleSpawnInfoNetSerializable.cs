// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class ObstacleSpawnInfoNetSerializable : INetSerializable
{
	public float Time { get; set; }
	public float StartBeat { get; set; }
	public float EndBeat { get; set; }
	public int LineIndex { get; set; }
	public NoteLineLayer LineLayer { get; set; }
	public float Duration { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public Vector3Serializable MoveOffset { get; set; }
	public float ObstacleWidth { get; set; }
	public float ObstacleHeight { get; set; }
	public float Rotation { get; set; }

	public ObstacleSpawnInfoNetSerializable(float time, float startBeat, float endBeat, int lineIndex, NoteLineLayer lineLayer, float duration, int width, int height, Vector3Serializable moveOffset, float obstacleWidth, float obstacleHeight, float rotation)
	{
		Time = time;
		StartBeat = startBeat;
		EndBeat = endBeat;
		LineIndex = lineIndex;
		LineLayer = lineLayer;
		Duration = duration;
		Width = width;
		Height = height;
		MoveOffset = moveOffset;
		ObstacleWidth = obstacleWidth;
		ObstacleHeight = obstacleHeight;
		Rotation = rotation;
	}

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteFloat(Time);
		writer.WriteVarInt(LineIndex);
		writer.WriteVarInt((int)LineLayer);
		writer.WriteFloat(Duration);
		writer.WriteVarInt(Width);
		writer.WriteVarInt(Height);
		writer.WriteSerializable<Vector3Serializable>(MoveOffset);
		writer.WriteFloat(ObstacleWidth);
		writer.WriteFloat(ObstacleHeight);
		writer.WriteFloat(Rotation);
	}

	public void ReadFrom(ref NetReader reader)
	{
		Time = reader.ReadFloat();
		LineIndex = reader.ReadVarInt();
		LineLayer = (NoteLineLayer)reader.ReadVarInt();
		Duration = reader.ReadFloat();
		Width = reader.ReadVarInt();
		Height = reader.ReadVarInt();
		MoveOffset = reader.ReadSerializable<Vector3Serializable>();
		ObstacleWidth = reader.ReadFloat();
		ObstacleHeight = reader.ReadFloat();
		Rotation = reader.ReadFloat();
	}
}