using System.Net;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.GameServer.BSSB;

public class LocalDiscoveryPacket : INetSerializable
{
    public IPEndPoint ServerEndPoint;
    public string ServerName;
    public string ServerUserId;
    public string GameModeName;
    public string ServerTypeName;
    public int PlayerCount;
    public BeatmapLevelSelectionMask BeatmapLevelSelectionMask;
    public GameplayServerConfiguration GameplayServerConfiguration;
    
    public void WriteTo(ref NetWriter writer)
    {
        writer.WriteString(LocalDiscovery.PacketPrefix);
        writer.WriteInt(LocalDiscovery.ProtocolVersion);
        
        writer.WriteIpEndPoint(ServerEndPoint);
        writer.WriteString(ServerName);
        writer.WriteString(ServerUserId);
        writer.WriteString(GameModeName);
        writer.WriteString(ServerTypeName);
        writer.WriteInt(PlayerCount);
        writer.WriteSerializable(BeatmapLevelSelectionMask);
        writer.WriteSerializable(GameplayServerConfiguration);
    }

    public void ReadFrom(ref NetReader reader)
    {
        throw new NotImplementedException("Server does not handle discovery responses");
    }
}