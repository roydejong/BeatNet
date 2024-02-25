using BeatNet.GameServer.Lobby;
using BeatNet.GameServer.Management;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatNet.GameServer.Main;

public class Service : IHostedService
{
    private readonly ILogger _logger;
    private readonly Config _config;

    private readonly PortAllocator _lobbyPortAllocator;
    private readonly Dictionary<ushort, LobbyHost> _lobbies;

    private bool _shuttingDown;
    
    private List<LobbyHost> SpareLobbies => _lobbies.Values.Where(x => x.IsEmpty).ToList();
    private int TotalLobbyCount => _lobbies.Count;
    private int SpareLobbyCount => SpareLobbies.Count;

    public Service(ILogger logger, Config config)
    {
        _logger = logger.ForContext<Service>();
        _config = config;

        _lobbyPortAllocator = new PortAllocator(config.PortRangeMin, config.PortRangeMax);
        _lobbies = new();
        
        _shuttingDown = false;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("BeatNet server is starting...");
        _shuttingDown = false;
        
        await EnsureSpareLobbies();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("BeatNet server is shutting down...");
        _shuttingDown = true;

        foreach (var lobby in _lobbies.Values)
        {
            await lobby.Stop();
        }
        
        _lobbies.Clear();
        _logger.Information("Shutdown complete. Bye!");
    }

    private async Task<LobbyHost?> StartLobby()
    {
        if (_shuttingDown)
            return null;

        if (TotalLobbyCount >= _config.MaxLobbyCount)
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
        
        while (SpareLobbyCount < _config.SpareLobbyCount && TotalLobbyCount < _config.MaxLobbyCount && !_lobbyPortAllocator.IsEmpty)
        {
            if (!didLog)
            {
                _logger.Information("Starting spare lobbies (need {X})...", _config.SpareLobbyCount);
                didLog = true;
            }

            var startOk = await StartLobby() != null;
            if (!startOk)
                break;
        }
    }
}