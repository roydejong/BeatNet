// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class PlayersMissingEntitlementsNetSerializable : INetSerializable
{
	public List<string> PlayersWithoutEntitlements { get; set; }

	public PlayersMissingEntitlementsNetSerializable(List<string> playersWithoutEntitlements)
	{
		PlayersWithoutEntitlements = playersWithoutEntitlements;
	}

	public void WriteTo(ref NetWriter writer)
	{
		// GenericListTypesFixedImpl
		writer.WriteStringList(PlayersWithoutEntitlements);
	}

	public void ReadFrom(ref NetReader reader)
	{
		// GenericListTypesFixedImpl
		PlayersWithoutEntitlements = reader.ReadStringList();
	}
}