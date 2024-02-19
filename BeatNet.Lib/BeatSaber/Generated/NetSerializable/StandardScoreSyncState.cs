// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class StandardScoreSyncState
{
	public int ModifiedScore { get; set; }
	public int MultipliedScore { get; set; }
	public int ImmediateMaxPossibleMultipliedScore { get; set; }
	public int Combo { get; set; }
	public int Multiplier { get; set; }

	public StandardScoreSyncState(int modifiedScore, int multipliedScore, int immediateMaxPossibleMultipliedScore, int combo, int multiplier)
	{
		ModifiedScore = modifiedScore;
		MultipliedScore = multipliedScore;
		ImmediateMaxPossibleMultipliedScore = immediateMaxPossibleMultipliedScore;
		Combo = combo;
		Multiplier = multiplier;
	}
}
