// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class GameplayModifiers
{
	public EnergyType EnergyType { get; set; }
	public bool NoFailOn0Energy { get; set; }
	public bool InstaFail { get; set; }
	public bool FailOnSaberClash { get; set; }
	public EnabledObstacleType EnabledObstacleType { get; set; }
	public bool FastNotes { get; set; }
	public bool StrictAngles { get; set; }
	public bool DisappearingArrows { get; set; }
	public bool GhostNotes { get; set; }
	public bool NoBombs { get; set; }
	public SongSpeed SongSpeed { get; set; }
	public bool NoArrows { get; set; }
	public bool ProMode { get; set; }
	public bool ZenMode { get; set; }
	public bool SmallCubes { get; set; }
	public float SongSpeedMul { get; set; }
	public float CutAngleTolerance { get; set; }
	public float NotesUniformScale { get; set; }
	public GameplayModifiers NoModifiers { get; set; }

	public GameplayModifiers(EnergyType energyType, bool noFailOn0Energy, bool instaFail, bool failOnSaberClash, EnabledObstacleType enabledObstacleType, bool fastNotes, bool strictAngles, bool disappearingArrows, bool ghostNotes, bool noBombs, SongSpeed songSpeed, bool noArrows, bool proMode, bool zenMode, bool smallCubes, float songSpeedMul, float cutAngleTolerance, float notesUniformScale, GameplayModifiers noModifiers)
	{
		EnergyType = energyType;
		NoFailOn0Energy = noFailOn0Energy;
		InstaFail = instaFail;
		FailOnSaberClash = failOnSaberClash;
		EnabledObstacleType = enabledObstacleType;
		FastNotes = fastNotes;
		StrictAngles = strictAngles;
		DisappearingArrows = disappearingArrows;
		GhostNotes = ghostNotes;
		NoBombs = noBombs;
		SongSpeed = songSpeed;
		NoArrows = noArrows;
		ProMode = proMode;
		ZenMode = zenMode;
		SmallCubes = smallCubes;
		SongSpeedMul = songSpeedMul;
		CutAngleTolerance = cutAngleTolerance;
		NotesUniformScale = notesUniformScale;
		NoModifiers = noModifiers;
	}
}