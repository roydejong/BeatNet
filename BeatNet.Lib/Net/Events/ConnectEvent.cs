using BeatNet.Lib.BeatSaber;
using ENet;

namespace BeatNet.Lib.Net.Events;

/// <summary>
/// Net server event: a peer has connected and sent a connection request.
/// </summary>
public class ConnectEvent
{
    /// <summary>
    /// The peer that is connecting.
    /// </summary>
    public readonly Peer Peer;
    /// <summary>
    /// The connection request that the peer is making.
    /// </summary>
    public readonly ConnectionRequest ConnectionRequest;

    public ConnectEvent(Peer peer, ConnectionRequest connectionRequest)
    {
        Peer = peer;
        ConnectionRequest = connectionRequest;
    }
}