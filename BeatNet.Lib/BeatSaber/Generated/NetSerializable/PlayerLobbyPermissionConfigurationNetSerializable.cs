// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class PlayerLobbyPermissionConfigurationNetSerializable : INetSerializable
{
	public string UserId { get; set; }
	public bool IsServerOwner { get; set; }
	public bool HasRecommendBeatmapsPermission { get; set; }
	public bool HasRecommendGameplayModifiersPermission { get; set; }
	public bool HasKickVotePermission { get; set; }
	public bool HasInvitePermission { get; set; }

	public PlayerLobbyPermissionConfigurationNetSerializable(string userId, bool isServerOwner, bool hasRecommendBeatmapsPermission, bool hasRecommendGameplayModifiersPermission, bool hasKickVotePermission, bool hasInvitePermission)
	{
		UserId = userId;
		IsServerOwner = isServerOwner;
		HasRecommendBeatmapsPermission = hasRecommendBeatmapsPermission;
		HasRecommendGameplayModifiersPermission = hasRecommendGameplayModifiersPermission;
		HasKickVotePermission = hasKickVotePermission;
		HasInvitePermission = hasInvitePermission;
	}

	public void WriteTo(ref NetWriter writer)
	{
		writer.WriteString(UserId);
		byte flags = 0;
		flags |= (byte)(IsServerOwner ? 1 : 0);
		flags |= (byte)(HasRecommendBeatmapsPermission ? 2 : 0);
		flags |= (byte)(HasRecommendGameplayModifiersPermission ? 4 : 0);
		flags |= (byte)(HasKickVotePermission ? 8 : 0);
		flags |= (byte)(HasInvitePermission ? 16 : 0);
		writer.WriteByte(flags);
	}

	public void ReadFrom(ref NetReader reader)
	{
		UserId = reader.ReadString();
		var flags = reader.ReadByte();
		IsServerOwner = (flags & 1) != 0;
		HasRecommendBeatmapsPermission = (flags & 2) != 0;
		HasRecommendGameplayModifiersPermission = (flags & 4) != 0;
		HasKickVotePermission = (flags & 8) != 0;
		HasInvitePermission = (flags & 16) != 0;
	}
}