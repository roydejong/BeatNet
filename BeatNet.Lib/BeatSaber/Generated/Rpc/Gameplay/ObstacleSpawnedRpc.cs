// This file was generated by BeatNet.CodeGen (RpcGenerator)
// Do not modify manually

using BeatNet.Lib.Net.IO;
using BeatNet.Lib.BeatSaber.Rpc;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Generated.Rpc.Gameplay;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global

public sealed class ObstacleSpawnedRpc : BaseRpc
{
	public override byte RpcType => (byte)GameplayRpcType.ObstacleSpawned;

	public float? SongTime { get; set; } = null;
	public ObstacleSpawnInfoNetSerializable? ObstacleSpawnInfoNetSerializable { get; set; } = null;

	public ObstacleSpawnedRpc(float? songTime = null, ObstacleSpawnInfoNetSerializable? obstacleSpawnInfoNetSerializable = null)
	{
		SongTime = songTime;
		ObstacleSpawnInfoNetSerializable = obstacleSpawnInfoNetSerializable;
	}

	public override void WriteTo(ref NetWriter writer)
	{
		base.WriteTo(ref writer);

		var nullFlags = (byte)(
			(SongTime != null ? 1 : 0) | 
			(ObstacleSpawnInfoNetSerializable != null ? 2 : 0)
		);

		writer.WriteByte(nullFlags);

		if (SongTime != null)
			writer.WriteFloat(SongTime.Value);

		if (ObstacleSpawnInfoNetSerializable != null)
			writer.WriteSerializable<ObstacleSpawnInfoNetSerializable>(ObstacleSpawnInfoNetSerializable);
	}

	public override void ReadFrom(ref NetReader reader)
	{
		base.ReadFrom(ref reader);

		var nullFlags = reader.ReadByte();

		if ((nullFlags & (1 << 0)) != 0)
			SongTime = reader.ReadFloat();
		else
			SongTime = null;

		if ((nullFlags & (1 << 1)) != 0)
			ObstacleSpawnInfoNetSerializable = reader.ReadSerializable<ObstacleSpawnInfoNetSerializable>();
		else
			ObstacleSpawnInfoNetSerializable = null;
	}
}