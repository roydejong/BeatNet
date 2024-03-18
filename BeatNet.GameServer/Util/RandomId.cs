namespace BeatNet.GameServer.Util;

public static class RandomId
{
    public static string Generate(int length = 22)
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..length];
    }
}