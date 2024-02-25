using BeatNet.Lib.BeatSaber.Generated.Enum;
using ENet;

namespace BeatNet.Lib.Net.Events;

/// <summary>
/// Net server event: peer should be disconnected or has disconnected.
/// </summary>
public class DisconnectEvent
{
    /// <summary>
    /// The peer that is disconnecting.
    /// </summary>
    public readonly uint PeerId;
    /// <summary>
    /// If true, the disconnect has been completed or should be completed immediately.
    /// If false, any pending data should be sent before the disconnect is completed (clean disconnect).
    /// </summary>
    public readonly bool Immediate;
    /// <summary>
    /// If applicable, the in-game reason for the disconnection.
    /// </summary>
    public readonly DisconnectedReason GameReason;
    
    public DisconnectEvent(uint peerId, bool immediate, DisconnectedReason gameReason = DisconnectedReason.Unknown)
    {
        PeerId = peerId;
        Immediate = immediate;
        GameReason = gameReason;
    }
}