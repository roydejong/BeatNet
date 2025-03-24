using Newtonsoft.Json;
using Serilog;

namespace BeatNet.GameServer.Management;

public class Config
{
    /// <summary>
    /// The UDP port the game server / lobby instance should use.
    /// If the environment variable "SERVER_PORT" is set, it will override this value.
    /// </summary>
    [JsonProperty] public ushort UdpPort = 7777;

    /// <summary>
    /// Enable local network discovery for server browser clients.
    /// </summary>
    [JsonProperty] public bool EnableLocalDiscovery = true;
    
    /// <summary>
    /// The server's public WAN (IPv4 or IPv6) address.
    /// Used in server browser announces, must be the address WAN users will use to connect.
    /// If null, WAN address will be automatically detected.
    /// </summary>
    [JsonProperty] public string? WanAddress = null;
    
    /// <summary>
    /// The absolute maximum amount of players per lobby instance.
    /// Should never exceed 127 due to the limitations of the multiplayer protocol.
    /// </summary>
    [JsonProperty] public int MaxPlayerCount = 32;

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