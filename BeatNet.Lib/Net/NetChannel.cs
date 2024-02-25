namespace BeatNet.Lib.Net;

public enum NetChannel : byte
{
    /// <summary>
    /// Reliable ENet channel (aka delivery method: ReliableOrdered).
    /// </summary>
    Reliable = 1,
    /// <summary>
    /// Unreliable / unsequenced ENet channel (aka delivery method: Unreliable).
    /// </summary>
    Unreliable = 2
}