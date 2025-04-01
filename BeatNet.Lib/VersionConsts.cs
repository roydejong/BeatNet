using JetBrains.Annotations;

namespace BeatNet.Lib;

public static class VersionConsts
{
    [PublicAPI] public const string GameVersionMinimum = "1.40.0";
    [PublicAPI] public const string GameVersionLabel = "1.40.4";
    [PublicAPI] public const string GameVersionMaximum = "1.40.999";
    [PublicAPI] public const string MultiplayerCoreVersion = "1.5.0";
    [PublicAPI] public const uint ProtocolVersion = 9U;

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