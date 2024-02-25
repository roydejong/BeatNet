using Newtonsoft.Json;
using Serilog;

namespace BeatNet.GameServer.Management;

public class Config
{
    /// <summary>
    /// The lower bound (inclusive) of the UDP port range to use for lobby instances.
    /// </summary>
    [JsonProperty] public ushort PortRangeMin = 7777;
    
    /// <summary>
    /// The upper bound (inclusive) of the UDP port range to use for lobby instances.
    /// </summary>
    [JsonProperty] public ushort PortRangeMax = 8888;
    
    /// <summary>
    /// The amount of spare lobby instances to keep ready at all times.
    /// Will never exceed the amount of available ports in the configured range.
    /// </summary>
    [JsonProperty] public int SpareLobbyCount => 1;
    
    /// <summary>
    /// The absolute maximum amount lobbies this server will host.
    /// </summary>
    [JsonProperty] public int MaxLobbyCount => 32;
    
    /// <summary>
    /// The absolute maximum amount of players per lobby instance.
    /// Should never exceed 127 due to the limitations of the multiplayer protocol.
    /// </summary>
    [JsonProperty] public int MaxPlayerCount => 32;

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