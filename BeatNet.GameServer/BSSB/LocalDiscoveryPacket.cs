using System.Net;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.GameServer.BSSB;

public class LocalDiscoveryPacket : INetSerializable
{
    public IPEndPoint ServerEndPoint;
    public int PlayerCount;
    public int PlayerLimit;
    public string ServerName;
    public string GameModeName;
    
    public void WriteTo(ref NetWriter writer)
    {
        writer.WriteString(LocalDiscovery.PacketPrefix);
        writer.WriteInt(LocalDiscovery.ProtocolVersion);
        writer.WriteIpEndPoint(ServerEndPoint);
        writer.WriteInt(PlayerCount);
        writer.WriteInt(PlayerLimit);
        writer.WriteString(ServerName);
        writer.WriteString(GameModeName);
    }

    public void ReadFrom(ref NetReader reader)
    {
        throw new NotImplementedException("Server does not handle discovery responses");
    }
}