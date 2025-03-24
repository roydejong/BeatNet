using System.Net;
using System.Net.Sockets;
using BeatNet.GameServer.Lobby;
using BeatNet.GameServer.Main;
using BeatNet.Lib;
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
        _logger?.Information("Started local network discovery ({Port})", BroadcastPort);

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
                var protocolVersion = reader.ReadInt();
                var gameVersion = reader.ReadString();

                if (prefix != PacketPrefix)
                    // Invalid packet: bad prefix
                    continue;

                if (protocolVersion != ProtocolVersion)
                {
                    // Warning: possibly incompatible protocol version
                    _logger?.Warning("Ignoring discovery packet with unknown protocol version {Version} ({Source})",
                        protocolVersion, clientEp);
                    continue;
                }

                if (!VersionConsts.IsGameVersionCompatible(gameVersion))
                {
                    // Unsupported game version
                    _logger?.Warning("Ignoring discovery packet with incompatible game version {Version} ({Source})",
                        gameVersion, clientEp);
                    continue;
                }

                _logger?.Debug("Sending local network discovery response to {ClientEp}", clientEp);

                // Send response packet for public lobby
                var publicLobbies = new List<LobbyHost>();
                if (_service.LobbyHost?.IsRunning ?? false)
                    publicLobbies.Add(_service.LobbyHost);

                foreach (var lobby in publicLobbies)
                {
                    var packet = new LocalDiscoveryPacket
                    {
                        Port = lobby.PortNumber,
                        ServerName = lobby.ServerName,
                        ServerUserId = lobby.ServerUserId,
                        GameModeName = lobby.GameModeName,
                        ServerTypeName = "BeatNet",
                        PlayerCount = lobby.PlayerCount,
                        LobbyState = (int)lobby.GameMode.LobbyState,
                        BeatmapLevelSelectionMask = lobby.GetBeatmapLevelSelectionMask(),
                        GameplayServerConfiguration = lobby.GetGameplayServerConfiguration()
                    };

                    writer.Reset();
                    packet.WriteTo(ref writer);

                    _udpClient.Send(writer.Data.ToArray(), writer.Position, clientEp);
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