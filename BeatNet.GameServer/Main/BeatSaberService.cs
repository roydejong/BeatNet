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

        LobbyHost = new LobbyHost(Config.UdpPort);
        LobbyHost.SetLogger(_logger);

        await LobbyHost.Start();

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