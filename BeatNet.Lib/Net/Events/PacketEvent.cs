namespace BeatNet.Lib.Net.Events;

/// <summary>
/// Net server event: packet received or packet pending send.
/// </summary>
public sealed class PacketEvent(uint peerId, NetChannel channel, NetPayload payload)
{
    public readonly uint PeerId = peerId;
    public readonly NetChannel Channel = channel;
    public readonly NetPayload Payload = payload;
}