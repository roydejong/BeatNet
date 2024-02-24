// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class PlayersLobbyPermissionConfigurationNetSerializable : INetSerializable
{
	public List<PlayerLobbyPermissionConfigurationNetSerializable> PlayersPermission { get; set; }

	public PlayersLobbyPermissionConfigurationNetSerializable(List<PlayerLobbyPermissionConfigurationNetSerializable> playersPermission)
	{
		PlayersPermission = playersPermission;
	}

	public void WriteTo(ref NetWriter writer)
	{
		// GenericListTypesFixedImpl
		writer.WriteSerializableList<List<PlayerLobbyPermissionConfigurationNetSerializable>, PlayerLobbyPermissionConfigurationNetSerializable>(PlayersPermission);
	}

	public void ReadFrom(ref NetReader reader)
	{
		// GenericListTypesFixedImpl
		PlayersPermission = reader.ReadSerializableList<List<PlayerLobbyPermissionConfigurationNetSerializable>, PlayerLobbyPermissionConfigurationNetSerializable>();
	}
}