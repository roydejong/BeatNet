using ENet;

namespace BeatNet.Lib.Net.Events;

/// <summary>
/// Net server event: packet received or packet pending send.
/// </summary>
public sealed class PacketEvent
{
    public readonly uint PeerId;
    public readonly NetChannel Channel;
    public readonly NetPayload Payload;
    public readonly bool Broadcast;

    public PacketEvent(uint peerId, NetChannel channel, NetPayload payload, bool broadcast = false)
    {
        PeerId = peerId;
        Channel = channel;
        Payload = payload;
        Broadcast = false;
    }
}