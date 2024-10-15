using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.MultiplayerCore.Serializable;

/// <summary>
/// MapColorSerializable implementation, as used by MultiplayerCore, but originally defined by SongCore as MapColor
/// Note: SongCore version has Alpha (A) as well but this does not get serialized so whatever
/// </summary>
/// <remarks>
/// https://github.com/Goobwabber/MultiplayerCore/blob/main/MultiplayerCore/Beatmaps/Serializable/DifficultyColors.cs
/// https://github.com/Kylemc1413/SongCore/blob/e584f037b0b4f5c339c24bc21892342e160f89c8/source/SongCore/Data/SongData.cs#L102
/// </remarks>
public class MapColor : INetSerializable
{
    public float R { get; set; } = 0f;
    public float G { get; set; } = 0f;
    public float B { get; set; } = 0f;
    
    public void WriteTo(ref NetWriter writer)
    {
        writer.WriteFloat(R);
        writer.WriteFloat(G);
        writer.WriteFloat(B);
    }

    public void ReadFrom(ref NetReader reader)
    {
        R = reader.ReadFloat();
        G = reader.ReadFloat();
        B = reader.ReadFloat();
    }
}