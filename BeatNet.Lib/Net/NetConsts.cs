namespace BeatNet.Lib.Net;

public static class NetConsts
{
    public const int MaximumChannels = 2;
    public const int MaximumPeers = 100;
    public const int MaximumPacketSize = 33554432; 
    public const int PollTime = 1;
    public const int IncomingOutgoingBufferSize = 5000;
    public const int ConnectionEventBufferSize = 100;
    public const int SubBufferCount = 10;
}