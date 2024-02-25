// This file was generated by BeatNet.CodeGen (PacketGenerator)
// Do not modify manually

using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Common;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Packet;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
public sealed class PlayerIdentityPacket : BaseCpmPacket
{
	public override InternalMessageType InternalMessageType => InternalMessageType.PlayerIdentity;

	public PlayerStateHash PlayerState { get; set; }
	public MultiplayerAvatarsData PlayerAvatar { get; set; }
	public ByteArrayNetSerializable Random { get; set; }
	public ByteArrayNetSerializable PublicEncryptionKey { get; set; }

	public PlayerIdentityPacket(PlayerStateHash playerState, MultiplayerAvatarsData playerAvatar, ByteArrayNetSerializable random, ByteArrayNetSerializable publicEncryptionKey)
	{
		PlayerState = playerState;
		PlayerAvatar = playerAvatar;
		Random = random;
		PublicEncryptionKey = publicEncryptionKey;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		writer.WriteSerializable<PlayerStateHash>(PlayerState);
		writer.WriteSerializable<MultiplayerAvatarsData>(PlayerAvatar);
		writer.WriteSerializable<ByteArrayNetSerializable>(Random);
		writer.WriteSerializable<ByteArrayNetSerializable>(PublicEncryptionKey);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		PlayerState = reader.ReadSerializable<PlayerStateHash>();
		PlayerAvatar = reader.ReadSerializable<MultiplayerAvatarsData>();
		Random = reader.ReadSerializable<ByteArrayNetSerializable>();
		PublicEncryptionKey = reader.ReadSerializable<ByteArrayNetSerializable>();
	}
}