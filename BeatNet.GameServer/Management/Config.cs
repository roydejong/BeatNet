using JetBrains.Annotations;
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

    /// <summary>
    /// If set to true, the lobby will be publicly listed in the server browser.
    /// </summary>
    [JsonProperty] public bool Public = false;
    
    /// <summary>
    /// The name to use for local discovery / server browser listings.
    /// </summary>
    [JsonProperty] public string Name = "BeatNet Server";

    [PublicAPI]
    public string ToJson() =>
        JsonConvert.SerializeObject(this, Formatting.Indented);

    [PublicAPI]
    public static Config? FromJson(string json) =>
        JsonConvert.DeserializeObject<Config>(json);

    [PublicAPI]
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

    [PublicAPI]
    public static Config? TryLoadFile(string path, bool noComplain = false)
    {
        var absolutePath = Path.GetFullPath(path);

        if (!File.Exists(absolutePath))
            return null;

        Config? config = null;

        try
        {
            config = FromJson(File.ReadAllText(absolutePath));
            Log.Logger.Information("Loaded config file: {Path}", absolutePath);
        }
        catch (Exception ex)
        {
            if (!noComplain)
                Log.Logger.Warning(ex, "Failed to load config file: {Path}", absolutePath);
        }

        return config;
    }
}