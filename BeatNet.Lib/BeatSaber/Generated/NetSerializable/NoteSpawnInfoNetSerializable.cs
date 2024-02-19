// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class NoteSpawnInfoNetSerializable
{
	public float Time { get; set; }
	public int LineIndex { get; set; }
	public NoteLineLayer NoteLineLayer { get; set; }
	public NoteLineLayer BeforeJumpNoteLineLayer { get; set; }
	public GameplayType GameplayType { get; set; }
	public ScoringType ScoringType { get; set; }
	public ColorType ColorType { get; set; }
	public NoteCutDirection CutDirection { get; set; }
	public float TimeToNextColorNote { get; set; }
	public float TimeToPrevColorNote { get; set; }
	public int FlipLineIndex { get; set; }
	public float FlipYSide { get; set; }
	public Vector3Serializable MoveStartPos { get; set; }
	public Vector3Serializable MoveEndPos { get; set; }
	public Vector3Serializable JumpEndPos { get; set; }
	public float JumpGravity { get; set; }
	public float MoveDuration { get; set; }
	public float JumpDuration { get; set; }
	public float Rotation { get; set; }
	public float CutDirectionAngleOffset { get; set; }
	public float CutSfxVolumeMultiplier { get; set; }

	public NoteSpawnInfoNetSerializable(float time, int lineIndex, NoteLineLayer noteLineLayer, NoteLineLayer beforeJumpNoteLineLayer, GameplayType gameplayType, ScoringType scoringType, ColorType colorType, NoteCutDirection cutDirection, float timeToNextColorNote, float timeToPrevColorNote, int flipLineIndex, float flipYSide, Vector3Serializable moveStartPos, Vector3Serializable moveEndPos, Vector3Serializable jumpEndPos, float jumpGravity, float moveDuration, float jumpDuration, float rotation, float cutDirectionAngleOffset, float cutSfxVolumeMultiplier)
	{
		Time = time;
		LineIndex = lineIndex;
		NoteLineLayer = noteLineLayer;
		BeforeJumpNoteLineLayer = beforeJumpNoteLineLayer;
		GameplayType = gameplayType;
		ScoringType = scoringType;
		ColorType = colorType;
		CutDirection = cutDirection;
		TimeToNextColorNote = timeToNextColorNote;
		TimeToPrevColorNote = timeToPrevColorNote;
		FlipLineIndex = flipLineIndex;
		FlipYSide = flipYSide;
		MoveStartPos = moveStartPos;
		MoveEndPos = moveEndPos;
		JumpEndPos = jumpEndPos;
		JumpGravity = jumpGravity;
		MoveDuration = moveDuration;
		JumpDuration = jumpDuration;
		Rotation = rotation;
		CutDirectionAngleOffset = cutDirectionAngleOffset;
		CutSfxVolumeMultiplier = cutSfxVolumeMultiplier;
	}
}