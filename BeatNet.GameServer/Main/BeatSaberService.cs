﻿using BeatNet.GameServer.BSSB;
using BeatNet.GameServer.Lobby;
using BeatNet.GameServer.Management;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatNet.GameServer.Main;

public class BeatSaberService : IHostedService
{
    private readonly ILogger _logger;
    public readonly Config Config;

    private readonly LocalDiscovery _localDiscovery;

    public LobbyHost? LobbyHost { get; private set; }

    public BeatSaberService(ILogger logger, Config config)
    {
        _logger = logger.ForContext<BeatSaberService>();

        Config = config;

        _localDiscovery = new(this);
        _localDiscovery.SetLogger(_logger);

        LobbyHost = null;
    }

    private ushort DetermineServerPort()
    {
        var port = Config.UdpPort;
        
        if (Environment.GetEnvironmentVariable("SERVER_PORT") is not { } envPort)
            return port;
        
        if (ushort.TryParse(envPort, out var parsedPort))
        {
            port = parsedPort;
            _logger.Information("Overriding UDP port with SERVER_PORT: {Port}", port);
        }
        else
        {
            _logger.Warning("Invalid SERVER_PORT value: {Port}", envPort);
        }

        return port;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("BeatNet server is starting...");
        
        LobbyHost = new LobbyHost(
            portNumber: DetermineServerPort(),
            maxPlayerCount: Config.MaxPlayerCount,
            gameMode: Config.GameMode
        );
        
        LobbyHost.SetLogger(_logger);

        var startResult = await LobbyHost.Start();

        if (!startResult)
        {
            await LobbyHost.Stop();
            _logger.Warning("Lobby host failed to start, exiting");
            Environment.Exit(-1);
            return;
        }

        if (Config.EnableLocalDiscovery)
            _localDiscovery.Start();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("BeatNet server is shutting down...");

        _localDiscovery.Stop();

        if (LobbyHost != null)
        {
            await LobbyHost.Stop();
            LobbyHost = null;
        }

        _logger.Information("Shutdown complete. Bye!");
    }
}