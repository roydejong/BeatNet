// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class LevelCompletionResults
{
	public GameplayModifiers GameplayModifiers { get; set; }
	public int ModifiedScore { get; set; }
	public int MultipliedScore { get; set; }
	public Rank Rank { get; set; }
	public bool FullCombo { get; set; }
	public float LeftSaberMovementDistance { get; set; }
	public float RightSaberMovementDistance { get; set; }
	public float LeftHandMovementDistance { get; set; }
	public float RightHandMovementDistance { get; set; }
	public LevelEndStateType LevelEndStateType { get; set; }
	public LevelEndAction LevelEndAction { get; set; }
	public float Energy { get; set; }
	public int GoodCutsCount { get; set; }
	public int BadCutsCount { get; set; }
	public int MissedCount { get; set; }
	public int NotGoodCount { get; set; }
	public int OkCount { get; set; }
	public int MaxCutScore { get; set; }
	public int TotalCutScore { get; set; }
	public int GoodCutsCountForNotesWithFullScoreScoringType { get; set; }
	public float AverageCenterDistanceCutScoreForNotesWithFullScoreScoringType { get; set; }
	public float AverageCutScoreForNotesWithFullScoreScoringType { get; set; }
	public int MaxCombo { get; set; }
	public float EndSongTime { get; set; }

	public LevelCompletionResults(GameplayModifiers gameplayModifiers, int modifiedScore, int multipliedScore, Rank rank, bool fullCombo, float leftSaberMovementDistance, float rightSaberMovementDistance, float leftHandMovementDistance, float rightHandMovementDistance, LevelEndStateType levelEndStateType, LevelEndAction levelEndAction, float energy, int goodCutsCount, int badCutsCount, int missedCount, int notGoodCount, int okCount, int maxCutScore, int totalCutScore, int goodCutsCountForNotesWithFullScoreScoringType, float averageCenterDistanceCutScoreForNotesWithFullScoreScoringType, float averageCutScoreForNotesWithFullScoreScoringType, int maxCombo, float endSongTime)
	{
		GameplayModifiers = gameplayModifiers;
		ModifiedScore = modifiedScore;
		MultipliedScore = multipliedScore;
		Rank = rank;
		FullCombo = fullCombo;
		LeftSaberMovementDistance = leftSaberMovementDistance;
		RightSaberMovementDistance = rightSaberMovementDistance;
		LeftHandMovementDistance = leftHandMovementDistance;
		RightHandMovementDistance = rightHandMovementDistance;
		LevelEndStateType = levelEndStateType;
		LevelEndAction = levelEndAction;
		Energy = energy;
		GoodCutsCount = goodCutsCount;
		BadCutsCount = badCutsCount;
		MissedCount = missedCount;
		NotGoodCount = notGoodCount;
		OkCount = okCount;
		MaxCutScore = maxCutScore;
		TotalCutScore = totalCutScore;
		GoodCutsCountForNotesWithFullScoreScoringType = goodCutsCountForNotesWithFullScoreScoringType;
		AverageCenterDistanceCutScoreForNotesWithFullScoreScoringType = averageCenterDistanceCutScoreForNotesWithFullScoreScoringType;
		AverageCutScoreForNotesWithFullScoreScoringType = averageCutScoreForNotesWithFullScoreScoringType;
		MaxCombo = maxCombo;
		EndSongTime = endSongTime;
	}
}