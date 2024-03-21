namespace BeatNet.Lib;

public class VersionConsts
{
    public const string GameVersionMinimum = "1.35.0";
    public const string GameVersionMaximum = "1.35.999";
    public const string MultiplayerCoreVersion = "1.5.0";
    public const uint ProtocolVersion = 9U;
    
    public static bool IsGameVersionCompatible(string gameVersion)
    {
        if (!Version.TryParse(gameVersion, out var gameVersionParsed))
            // Invalid version
            return false;

        var minVersion = Version.Parse(GameVersionMinimum);
        var maxVersion = Version.Parse(GameVersionMaximum);
        
        return gameVersionParsed >= minVersion && gameVersionParsed <= maxVersion;
    }
}