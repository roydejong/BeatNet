// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class PlayerSpecificSettingsAtStartNetSerializable : INetSerializable
{
	public List<IConnectedPlayer> ActivePlayersAtGameStart { get; set; }
	public IReadOnlyList<PlayerSpecificSettingsNetSerializable> ActivePlayerSpecificSettingsAtGameStart { get; set; }

	public PlayerSpecificSettingsAtStartNetSerializable(List<IConnectedPlayer> activePlayersAtGameStart, IReadOnlyList<PlayerSpecificSettingsNetSerializable> activePlayerSpecificSettingsAtGameStart)
	{
		ActivePlayersAtGameStart = activePlayersAtGameStart;
		ActivePlayerSpecificSettingsAtGameStart = activePlayerSpecificSettingsAtGameStart;
	}

	public void WriteTo(ref NetWriter writer)
	{
		throw new NotImplementedException(); // TODO
	}

	public void ReadFrom(ref NetReader reader)
	{
		throw new NotImplementedException(); // TODO
	}
}