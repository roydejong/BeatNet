using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using JetBrains.Annotations;

namespace BeatNet.Lib.SongCore;

/// <summary>
/// https://github.com/Kylemc1413/SongCore/blob/master/source/SongCore/Data/SongData.cs#L38
/// </summary>
[UsedImplicitly]
public class Contributor : INetSerializable
{
    public string Role { get; set; } = "";
    public string Name { get; set; } = "";
    public string IconPath { get; set; } = "";
    
    public void WriteTo(ref NetWriter writer)
    {
        writer.WriteString(Role);
        writer.WriteString(Name);
        writer.WriteString(IconPath);
    }

    public void ReadFrom(ref NetReader reader)
    {
        Role = reader.ReadString();
        Name = reader.ReadString();
        IconPath = reader.ReadString();
    }
}