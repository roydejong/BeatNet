using System.Net;
using BeatNet.GameServer.BSSB;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("BeatNet server is starting...");
        
        // Config overrides
        var serverPort = DetermineServerPort();
        var serverPublic = DetermineServerPublic();
        var serverName = DetermineServerName();
        
        // Determine WAN address (needed for public servers)
        IPAddress? wanAddress = null;

        if (!string.IsNullOrEmpty(Config.WanAddress))
            wanAddress = IPAddress.Parse(Config.WanAddress);
        else if (serverPublic)
            wanAddress = await SelfIpUtil.TryGetWanAddress();

        if (wanAddress == null && serverPublic)
        {
            _logger.Error("Failed to determine WAN address, cannot start public server");
            Environment.Exit(-1);
            return;
        }
        
        // Start lobby
        LobbyHost = new LobbyHost(
            portNumber: serverPort,
            wanAddress: wanAddress,
            maxPlayerCount: Config.MaxPlayerCount,
            gameMode: Config.GameMode,
            serverName: serverName,
            isPublic: serverPublic,
            password: null, // (not yet supported anywhere)
            motd: Config.WelcomeMessage
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

        // Start local discovery
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

    #region Config override helpers
    
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

    private bool DetermineServerPublic()
    {
        var isPublic = Config.Public;

        if (Environment.GetEnvironmentVariable("PUBLIC") is { } envPublic)
        {
            if (envPublic.ToLowerInvariant() is "1" or "true")
            {
                isPublic = true;
                _logger.Information("Overriding public server setting with PUBLIC: {IsPublic}", isPublic);
            }
            else if (envPublic.ToLowerInvariant() is "0" or "false")
            {
                isPublic = false;
                _logger.Information("Overriding public server setting with PUBLIC: {IsPublic}", isPublic);
            }
            else
            {
                _logger.Warning("Invalid PUBLIC value: {Value}", envPublic);
            }
        }

        return isPublic;
    }

    private string DetermineServerName()
    {
        var name = Config.Name;

        if (Environment.GetEnvironmentVariable("NAME") is { } envName)
        {
            name = envName;
            _logger.Information("Overriding server name with NAME: {Name}", name);
        }

        return name;
    }

    #endregion
}