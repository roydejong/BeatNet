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

    private readonly PortAllocator _lobbyPortAllocator;
    private readonly Dictionary<ushort, LobbyHost> _lobbies;
    
    private readonly LocalDiscovery _localDiscovery; 

    private bool _shuttingDown;
    
    private List<LobbyHost> SpareLobbies => _lobbies.Values.Where(x => x.IsEmpty).ToList();
    private int TotalLobbyCount => _lobbies.Count;
    private int SpareLobbyCount => SpareLobbies.Count;

    public BeatSaberService(ILogger logger, Config config)
    {
        _logger = logger.ForContext<BeatSaberService>();
        
        Config = config;

        _lobbyPortAllocator = new PortAllocator(config.PortRangeMin, config.PortRangeMax);
        _lobbies = new();
        
        _localDiscovery = new(this);
        _localDiscovery.SetLogger(_logger);
        
        _shuttingDown = false;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("BeatNet server is starting...");
        _shuttingDown = false;
        
        await EnsureSpareLobbies();
        
        if (Config.EnableLocalDiscovery)
            _localDiscovery.Start();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("BeatNet server is shutting down...");
        _shuttingDown = true;
        
        _localDiscovery.Stop();
        
        var stopTasks = new List<Task>();
        foreach (var lobby in _lobbies.Values)
            stopTasks.Add(lobby.Stop());
        await Task.WhenAll(stopTasks);
        
        _lobbies.Clear();
        _logger.Information("Shutdown complete. Bye!");
    }

    private async Task<LobbyHost?> StartLobby()
    {
        if (_shuttingDown)
            return null;

        if (TotalLobbyCount >= Config.MaxLobbyCount)
        {
            _logger?.Warning("Could not start lobby: max lobby count reached");
            return null;
        }
        
        if (!_lobbyPortAllocator.TryAcquire(out var port))
        {
            _logger?.Warning("Could not start lobby: no ports available");
            return null;
        }
        
        var lobby = new LobbyHost(port);
        lobby.SetLogger(_logger);

        if (await lobby.Start())
        {
            _lobbies.Add(port, lobby);
            return lobby;
        }

        return null;
    }
    
    private async Task EnsureSpareLobbies()
    {
        if (_shuttingDown)
            return;
        
        var didLog = false;
        
        while (SpareLobbyCount < Config.SpareLobbyCount && TotalLobbyCount < Config.MaxLobbyCount && !_lobbyPortAllocator.IsEmpty)
        {
            if (!didLog)
            {
                _logger.Information("Starting spare lobbies (need {X})...", Config.SpareLobbyCount);
                didLog = true;
            }

            var startOk = await StartLobby() != null;
            if (!startOk)
                break;
        }
    }

    public IReadOnlyList<LobbyHost> GetPublicLobbies()
    {
        return _lobbies.Values.Where(x => x.IsPublic).ToList();
    }
}