namespace BeatNet.Lib.Net;

// Note: Beat Saber has a similar internal enum called DeliveryMethod, but the byte values are swapped...
// These correspond to the actual ENet channel IDs that the game uses.

public enum NetChannel : byte
{
    ReliableOrdered = 0,
    Unreliable = 1
}