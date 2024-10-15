using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;
using JetBrains.Annotations;

namespace BeatNet.Lib.MultiplayerCore.Serializable;

/// <summary>
/// Serializable map color set for a specific difficulty level.
/// As defined and serialized by MultiplayerCore. 
/// </summary>
/// <remarks>
/// https://github.com/Goobwabber/MultiplayerCore/blob/main/MultiplayerCore/Beatmaps/Serializable/DifficultyColors.cs
/// </remarks>
// ReSharper disable MemberCanBePrivate.Global
[UsedImplicitly]
public class DifficultyColors : INetSerializable
{
    public MapColor? ColorLeft { get; set; } = null;
    public MapColor? ColorRight { get; set; } = null;
    public MapColor? EnvColorLeft { get; set; } = null;
    public MapColor? EnvColorRight { get; set; } = null;
    public MapColor? EnvColorLeftBoost { get; set; } = null;
    public MapColor? EnvColorRightBoost { get; set; } = null;
    public MapColor? ObstacleColor { get; set; } = null;

    public void WriteTo(ref NetWriter writer)
    {
        // Bitmask for which colors are present
        var colors = (byte)(ColorLeft != null ? 1 : 0);
        colors |= (byte)((ColorRight != null ? 1 : 0) << 1);
        colors |= (byte)((EnvColorLeft != null ? 1 : 0) << 2);
        colors |= (byte)((EnvColorRight != null ? 1 : 0) << 3);
        colors |= (byte)((EnvColorLeftBoost != null ? 1 : 0) << 4);
        colors |= (byte)((EnvColorRightBoost != null ? 1 : 0) << 5);
        colors |= (byte)((ObstacleColor != null ? 1 : 0) << 6);
        writer.WriteByte(colors);

        // Write the colors
        if (ColorLeft != null)
            writer.WriteSerializable(ColorLeft);
        if (ColorRight != null)
            writer.WriteSerializable(ColorRight);
        if (EnvColorLeft != null)
            writer.WriteSerializable(EnvColorLeft);
        if (EnvColorRight != null)
            writer.WriteSerializable(EnvColorRight);
        if (EnvColorLeftBoost != null)
            writer.WriteSerializable(EnvColorLeftBoost);
        if (EnvColorRightBoost != null)
            writer.WriteSerializable(EnvColorRightBoost);
        if (ObstacleColor != null)
            writer.WriteSerializable(ObstacleColor);
    }

    public void ReadFrom(ref NetReader reader)
    {
        // Read the colors
        var colors = reader.ReadByte();

        if ((colors & 1) != 0)
            ColorLeft = reader.ReadSerializable<MapColor>();
        if ((colors & 2) != 0)
            ColorRight = reader.ReadSerializable<MapColor>();
        if ((colors & 4) != 0)
            EnvColorLeft = reader.ReadSerializable<MapColor>();
        if ((colors & 8) != 0)
            EnvColorRight = reader.ReadSerializable<MapColor>();
        if ((colors & 16) != 0)
            EnvColorLeftBoost = reader.ReadSerializable<MapColor>();
        if ((colors & 32) != 0)
            EnvColorRightBoost = reader.ReadSerializable<MapColor>();
        if ((colors & 64) != 0)
            ObstacleColor = reader.ReadSerializable<MapColor>();
    }
}