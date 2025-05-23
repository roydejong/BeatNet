// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class NoteCutInfoNetSerializable : INetSerializable
{
	public float SaberSpeed { get; set; }
	public bool CutWasOk { get; set; }
	public Vector3Serializable SaberDir { get; set; }
	public Vector3Serializable CutPoint { get; set; }
	public Vector3Serializable CutNormal { get; set; }
	public Vector3Serializable NotePosition { get; set; }
	public Vector3Serializable NoteScale { get; set; }
	public QuaternionSerializable NoteRotation { get; set; }
	public GameplayType GameplayType { get; set; }
	public ColorType ColorType { get; set; }
	public float NoteTime { get; set; }
	public int NoteLineIndex { get; set; }
	public NoteLineLayer LineLayer { get; set; }
	public float TimeToNextColorNote { get; set; }
	public Vector3Serializable MoveVec { get; set; }

	public NoteCutInfoNetSerializable(float saberSpeed, bool cutWasOk, Vector3Serializable saberDir, Vector3Serializable cutPoint, Vector3Serializable cutNormal, Vector3Serializable notePosition, Vector3Serializable noteScale, QuaternionSerializable noteRotation, GameplayType gameplayType, ColorType colorType, float noteTime, int noteLineIndex, NoteLineLayer lineLayer, float timeToNextColorNote, Vector3Serializable moveVec)
	{
		SaberSpeed = saberSpeed;
		CutWasOk = cutWasOk;
		SaberDir = saberDir;
		CutPoint = cutPoint;
		CutNormal = cutNormal;
		NotePosition = notePosition;
		NoteScale = noteScale;
		NoteRotation = noteRotation;
		GameplayType = gameplayType;
		ColorType = colorType;
		NoteTime = noteTime;
		NoteLineIndex = noteLineIndex;
		LineLayer = lineLayer;
		TimeToNextColorNote = timeToNextColorNote;
		MoveVec = moveVec;
	}

	public void WriteTo(ref NetWriter writer)
	{
		byte flags = 0;
		flags |= (byte)(CutWasOk ? 1 : 0);
		writer.WriteByte(flags);
		writer.WriteFloat(SaberSpeed);
		writer.WriteSerializable<Vector3Serializable>(SaberDir);
		writer.WriteSerializable<Vector3Serializable>(CutPoint);
		writer.WriteSerializable<Vector3Serializable>(CutNormal);
		writer.WriteSerializable<Vector3Serializable>(NotePosition);
		writer.WriteSerializable<Vector3Serializable>(NoteScale);
		writer.WriteSerializable<QuaternionSerializable>(NoteRotation);
		writer.WriteVarInt((int)GameplayType);
		writer.WriteVarInt((int)ColorType);
		writer.WriteVarInt((int)LineLayer);
		writer.WriteVarInt(NoteLineIndex);
		writer.WriteFloat(NoteTime);
		writer.WriteFloat(TimeToNextColorNote);
		writer.WriteSerializable<Vector3Serializable>(MoveVec);
	}

	public void ReadFrom(ref NetReader reader)
	{
		var flags = reader.ReadByte();
		CutWasOk = (flags & 1) != 0;
		SaberSpeed = reader.ReadFloat();
		SaberDir = reader.ReadSerializable<Vector3Serializable>();
		CutPoint = reader.ReadSerializable<Vector3Serializable>();
		CutNormal = reader.ReadSerializable<Vector3Serializable>();
		NotePosition = reader.ReadSerializable<Vector3Serializable>();
		NoteScale = reader.ReadSerializable<Vector3Serializable>();
		NoteRotation = reader.ReadSerializable<QuaternionSerializable>();
		GameplayType = (GameplayType)reader.ReadVarInt();
		ColorType = (ColorType)reader.ReadVarInt();
		LineLayer = (NoteLineLayer)reader.ReadVarInt();
		NoteLineIndex = reader.ReadVarInt();
		NoteTime = reader.ReadFloat();
		TimeToNextColorNote = reader.ReadFloat();
		MoveVec = reader.ReadSerializable<Vector3Serializable>();
	}
}