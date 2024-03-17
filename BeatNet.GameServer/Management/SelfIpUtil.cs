using System.Net;
using System.Net.Sockets;

namespace BeatNet.GameServer.Management;

public static class SelfIpUtil
{
    public static async Task<IPAddress?> TryGetWanAddress()
    {
        return await TryICanHaz() ?? await TryDynDns();
    }

    public static IPAddress? TryGetLanAddress()
    {
        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            try
            {
                socket.Connect("8.8.8.8", 65530);
            }
            catch (Exception)
            {
                // Connection may fail
            }

            try
            {
                if (socket.LocalEndPoint is IPEndPoint endPoint)
                    return endPoint.Address;
            }
            catch (Exception e)
            {
                // Socket may no longer be available
            }
        }

        return null;
    }

    private static async Task<string?> DownloadString(string httpAddress, bool trim = true)
    {
        var response = await new HttpClient().GetAsync(httpAddress);
        var str = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(str))
            return null;

        return str.Replace("\\r\\n", "").Replace("\\n", "").Trim();
    }

    private static async Task<IPAddress?> TryICanHaz()
    {
        try
        {
            var ipString = await DownloadString("https://icanhazip.com");

            if (!string.IsNullOrEmpty(ipString))
                return IPAddress.Parse(ipString);
        }
        catch (Exception)
        {
            // Request failure
        }

        return null;
    }

    private static async Task<IPAddress?> TryDynDns()
    {
        try
        {
            var ipString = await DownloadString("http://checkip.dyndns.org");

            if (!string.IsNullOrEmpty(ipString))
            {
                // Trim "Current IP Address: " prefix
                var prefixIdx = ipString.IndexOf(':');
                ipString = ipString.Substring(prefixIdx).Trim();

                return IPAddress.Parse(ipString);
            }
        }
        catch (Exception)
        {
            // Request failure
        }

        return null;
    }
}