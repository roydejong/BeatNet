﻿using System.Net;
using System.Net.Sockets;
using BeatNet.GameServer.Main;
using BeatNet.GameServer.Management;
using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;
using BeatNet.Lib.Net.IO;
using Serilog;

namespace BeatNet.GameServer.BSSB;

public class LocalDiscovery
{
    public const int ProtocolVersion = 1;
    public const string PacketPrefix = "BssbDiscovery";
    public const int BroadcastPort = 47777;

    private readonly BeatSaberService _service;
    private UdpClient? _udpClient;
    private Thread _pollThread;
    private bool _keepAlive;
    private ILogger? _logger;

    public bool IsActive => _keepAlive;

    public LocalDiscovery(BeatSaberService service)
    {
        _service = service;
        _pollThread = new Thread(__Poll);
    }

    public void SetLogger(ILogger logger)
    {
        _logger = logger
            .ForContext<LocalDiscovery>();
    }

    public void Start()
    {
        if (_keepAlive)
            return;
        
        _keepAlive = true;
        _udpClient = new UdpClient(BroadcastPort);
        _pollThread.Start();
    }

    public void Stop()
    {
        if (!_keepAlive)
            return;
        
        _keepAlive = false;
        _udpClient!.Dispose();
        _udpClient = null;
        _pollThread.Join();
        _logger?.Information("Stopped local network discovery");
    }

    private void __Poll()
    {
        // Determine LAN address for broadcast
        IPAddress? lanAddress = null;
        
        if (!string.IsNullOrEmpty(_service.Config.LanAddress))
            IPAddress.TryParse(_service.Config.LanAddress, out lanAddress);
        
        if (lanAddress == null)
            lanAddress = SelfIpUtil.TryGetLanAddress();
        
        if (lanAddress == null)
        {
            _logger?.Error("Local network discovery could not start, no LAN address could be determined");
            Stop();
            return;
        }
        
        _logger?.Information("Started local network discovery (LAN address: {LanAddress})", lanAddress);
        
        // Init write buffer
        var writeBuffer = GC.AllocateArray<byte>(1024, true);
        var writer = new NetWriter(writeBuffer);

        // Poll for discovery packets and respond with public lobbies
        while (_keepAlive)
        {
            try
            {
                var clientEp = new IPEndPoint(IPAddress.Any, BroadcastPort);
                var received = _udpClient!.Receive(ref clientEp);

                var reader = new NetReader(received);
                var prefix = reader.ReadString();

                if (prefix != PacketPrefix)
                    // Invalid packet: bad prefix
                    continue;

                var version = reader.ReadInt();
                if (version != ProtocolVersion)
                    // Warning: possibly incompatible protocol version
                    _logger?.Warning("Received discovery packet with unknown version {Version} ({Source})",
                        version, clientEp);
                
                _logger?.Information("Got discovery packet from {ClientEp}!", clientEp);
                
                // Helper: If the LAN address it the same as ours, treat as localhost
                var effectiveLanAddress = lanAddress;
                if (clientEp.Address.Equals(lanAddress))
                    effectiveLanAddress = IPAddress.Loopback;
                
                // Send response packet for every public lobby
                var publicLobbies = _service.GetPublicLobbies();
                foreach (var lobby in publicLobbies)
                {
                    var packet = new LocalDiscoveryPacket
                    {
                        ServerEndPoint = new IPEndPoint(effectiveLanAddress, lobby.PortNumber),
                        ServerName = lobby.ServerName,
                        ServerUserId = lobby.ServerUserId,
                        GameModeName = lobby.GameModeName,
                        PlayerCount = lobby.PlayerCount,
                        BeatmapLevelSelectionMask = lobby.GetBeatmapLevelSelectionMask(),
                        GameplayServerConfiguration = lobby.GetGameplayServerConfiguration()
                    };
                    
                    writer.Reset();
                    packet.WriteTo(ref writer);
                    _udpClient.Send(writer.Data.ToArray(), writer.Position, clientEp);
                
                    _logger?.Information("Sent discovery packet for a lobby!");
                }
            }
            catch (Exception ex)
            {
                if (_keepAlive)
                    _logger?.Error(ex, "Error in local discovery poll");
            }
        }
    }
}