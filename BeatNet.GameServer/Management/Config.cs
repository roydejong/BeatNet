using Newtonsoft.Json;
using Serilog;

namespace BeatNet.GameServer.Management;

public class Config
{
    /// <summary>
    /// The UDP port number the lobby should be hosted on.
    /// Can be overriden with the "SERVER_PORT" environment variable.
    /// </summary>
    [JsonProperty] public ushort UdpPort = 7777;

    /// <summary>
    /// If true, enable local network discovery responses (LAN discovery) for the Server Browser.
    /// </summary>
    [JsonProperty] public bool EnableLocalDiscovery = true;
    
    /// <summary>
    /// Manual override for the server's public IPv4 or IPv6 address.
    /// Used for public server browser lobbies only, this is the address clients will connect to.
    /// If not set, WAN address can be automatically detected.
    /// </summary>
    [JsonProperty] public string? WanAddress = null;
    
    /// <summary>
    /// The lobby size; how many players can join.
    /// Normally 2-5; modded lobbies support any player count up to 127.
    /// </summary>
    [JsonProperty] public int MaxPlayerCount = 5;

    /// <summary>
    /// The name of a supported game mode the lobby will run.
    /// </summary>
    [JsonProperty] public string GameMode = "beatnet:quickplay";

    public string ToJson() =>
        JsonConvert.SerializeObject(this, Formatting.Indented);

    public static Config? FromJson(string json) =>
        JsonConvert.DeserializeObject<Config>(json);

    public static Config LoadOrInitializeFile(string path)
    {
        var config = TryLoadFile(path);

        if (config != null)
            return config;

        config = new Config();

        try
        {
            var directory = new FileInfo(path).DirectoryName;
            if (directory != null)
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, config.ToJson());

            Log.Logger.Information("Created default config file: {Path}", path);
        }
        catch (Exception)
        {
            Log.Logger.Information("Could not create default config file: {Path}", path);
        }

        return config;
    }

    public static Config? TryLoadFile(string path, bool noComplain = false)
    {
        if (!File.Exists(path))
            return null;

        Config? config = null;

        try
        {
            config = FromJson(File.ReadAllText(path));
            Log.Logger.Information("Loaded config file: {Path}", path);
        }
        catch (Exception ex)
        {
            if (!noComplain)
                Log.Logger.Warning("Failed to load {Path}, error: {Exception}", path, ex);
        }

        return config;
    }
}