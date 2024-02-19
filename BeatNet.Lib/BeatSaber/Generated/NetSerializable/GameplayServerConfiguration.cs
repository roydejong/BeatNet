// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class GameplayServerConfiguration
{
	public int MaxPlayerCount { get; set; }
	public DiscoveryPolicy DiscoveryPolicy { get; set; }
	public InvitePolicy InvitePolicy { get; set; }
	public GameplayServerMode GameplayServerMode { get; set; }
	public SongSelectionMode SongSelectionMode { get; set; }
	public GameplayServerControlSettings GameplayServerControlSettings { get; set; }

	public GameplayServerConfiguration(int maxPlayerCount, DiscoveryPolicy discoveryPolicy, InvitePolicy invitePolicy, GameplayServerMode gameplayServerMode, SongSelectionMode songSelectionMode, GameplayServerControlSettings gameplayServerControlSettings)
	{
		MaxPlayerCount = maxPlayerCount;
		DiscoveryPolicy = discoveryPolicy;
		InvitePolicy = invitePolicy;
		GameplayServerMode = gameplayServerMode;
		SongSelectionMode = songSelectionMode;
		GameplayServerControlSettings = gameplayServerControlSettings;
	}
}