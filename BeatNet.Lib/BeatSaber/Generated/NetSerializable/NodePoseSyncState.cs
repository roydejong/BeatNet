// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)
// Do not modify manually

using System;
using BeatNet.Lib.BeatSaber.Generated.Enum;

namespace BeatNet.Lib.BeatSaber.Generated.NetSerializable;

public sealed class NodePoseSyncState
{
	public PoseSerializable Head { get; set; }
	public PoseSerializable LeftController { get; set; }
	public PoseSerializable RightController { get; set; }

	public NodePoseSyncState(PoseSerializable head, PoseSerializable leftController, PoseSerializable rightController)
	{
		Head = head;
		LeftController = leftController;
		RightController = rightController;
	}
}