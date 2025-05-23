﻿using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.GameServer.BSSB;

public class LocalDiscoveryPacket : INetSerializable
{
    public int Port;
    public string ServerName;
    public string ServerUserId;
    public string GameModeName;
    public string ServerTypeName;
    public int PlayerCount;
    public int LobbyState;
    public BeatmapLevelSelectionMask BeatmapLevelSelectionMask;
    public GameplayServerConfiguration GameplayServerConfiguration;
    
    public void WriteTo(ref NetWriter writer)
    {
        writer.WriteString(LocalDiscovery.PacketPrefix);
        writer.WriteInt(LocalDiscovery.ProtocolVersion);
        
        writer.WriteInt(Port);
        writer.WriteString(ServerName);
        writer.WriteString(ServerUserId);
        writer.WriteString(GameModeName);
        writer.WriteString(ServerTypeName);
        writer.WriteInt(PlayerCount);
        writer.WriteInt(LobbyState);
        writer.WriteSerializable(BeatmapLevelSelectionMask);
        writer.WriteSerializable(GameplayServerConfiguration);
    }

    public void ReadFrom(ref NetReader reader)
    {
        throw new NotImplementedException("Server does not handle discovery responses");
    }
}