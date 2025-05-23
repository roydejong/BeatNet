// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class GameplayServerConfiguration : INetSerializable
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

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteVarInt((int)MaxPlayerCount);
		writer.WriteVarInt((int)DiscoveryPolicy);
		writer.WriteVarInt((int)InvitePolicy);
		writer.WriteVarInt((int)GameplayServerMode);
		writer.WriteVarInt((int)SongSelectionMode);
		writer.WriteVarInt((int)GameplayServerControlSettings);
	}

	public void ReadFrom(ref NetReader reader)
	{
		MaxPlayerCount = (int)reader.ReadVarInt();
		DiscoveryPolicy = (DiscoveryPolicy)reader.ReadVarInt();
		InvitePolicy = (InvitePolicy)reader.ReadVarInt();
		GameplayServerMode = (GameplayServerMode)reader.ReadVarInt();
		SongSelectionMode = (SongSelectionMode)reader.ReadVarInt();
		GameplayServerControlSettings = (GameplayServerControlSettings)reader.ReadVarInt();
	}
}