using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber;

public class ConnectionRequest : INetSerializable
{
    public const string FixedPrefix = "IgnCon";
    
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public bool IsConnectionOwner { get; set; }
    public string? PlayerSessionId { get; set; }
    
    public bool IsValid => UserId != null && UserName != null && PlayerSessionId != null && !IsConnectionOwner;
    
    public void WriteTo(ref NetWriter writer)
    {
        writer.WriteString(FixedPrefix);
        writer.WriteString(UserId);
        writer.WriteString(UserName);
        writer.WriteBool(IsConnectionOwner);
        writer.WriteString(PlayerSessionId);
    }

    public void ReadFrom(ref NetReader reader)
    {
        if (!reader.TryReadString(out var prefix) || prefix != FixedPrefix)
            throw new InvalidDataException("Invalid connection request prefix");
        UserId = reader.ReadString();
        UserName = reader.ReadString();
        IsConnectionOwner = reader.ReadBool();
        PlayerSessionId = reader.ReadString();
    }
}