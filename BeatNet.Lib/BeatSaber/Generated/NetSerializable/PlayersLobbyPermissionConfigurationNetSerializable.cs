// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class PlayersLobbyPermissionConfigurationNetSerializable
{
	public List<PlayerLobbyPermissionConfigurationNetSerializable> PlayersPermission { get; set; }

	public PlayersLobbyPermissionConfigurationNetSerializable(List<PlayerLobbyPermissionConfigurationNetSerializable> playersPermission)
	{
		PlayersPermission = playersPermission;
	}
}
