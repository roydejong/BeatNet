using System.Diagnostics;
using System.Reflection;
using BeatNet.Lib;

namespace BeatNet.GameServer.Util;

public static class ServerVersion
{
    public static string ProductVersion => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location)
        .ProductVersion!;

    public static string ProductVersionShortHash
    {
        get
        {
            // Full version includes commit hash, ex 1.0.0+ce194aa18151dc94b01e0759da67a3d82b15f841
            var versionText = ProductVersion;

            // Shorten the git hash to 7 characters
            if (versionText.Contains('+'))
            {
                var versionParts = versionText.Split('+');

                if (versionParts.Length > 1)
                    versionText = $"{versionParts[0]}+{versionParts[1].Substring(0, 7)}";
            }

            return versionText;
        }
    }

    public static string UserAgent => $"BeatNet/{ProductVersionShortHash} (BeatSaber/{VersionConsts.GameVersionLabel})";
}