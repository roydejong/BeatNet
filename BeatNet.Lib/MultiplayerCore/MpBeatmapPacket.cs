using BeatNet.Lib.BeatSaber.Generated.Enum;
using BeatNet.Lib.MultiplayerCore.Serializable;
using BeatNet.Lib.Net.IO;
using BeatNet.Lib.SongCore;
using JetBrains.Annotations;

namespace BeatNet.Lib.MultiplayerCore;

// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global
[UsedImplicitly]
public class MpBeatmapPacket : BaseMpCorePacket
{
    public override MpCoreMessageType MpCoreMessageType => MpCoreMessageType.MpBeatmapPacket;
    public override string PacketName => "MpBeatmapPacket";

    public string LevelHash { get; set; } = null!;
    public string SongName { get; set; } = null!;
    public string SongSubName { get; set; } = null!;
    public string SongAuthorName { get; set; } = null!;
    public string LevelAuthorName { get; set; } = null!;
    public float BeatsPerMinute { get; set; }
    public float SongDuration { get; set; }
    public string CharacteristicName { get; set; } = null!;
    public BeatmapDifficulty Difficulty { get; set; }

    public Dictionary<BeatmapDifficulty, string[]> Requirements { get; set; } = new();
    public Contributor[]? Contributors { get; set; } = null;
    public Dictionary<BeatmapDifficulty, DifficultyColors> MapColors { get; set; } = new();

    public override void WriteTo(ref NetWriter writer)
    {
        // Main
        writer.WriteString(LevelHash);
        writer.WriteString(SongName);
        writer.WriteString(SongSubName);
        writer.WriteString(SongAuthorName);
        writer.WriteString(LevelAuthorName);
        writer.WriteFloat(BeatsPerMinute);
        writer.WriteFloat(SongDuration);
        writer.WriteString(CharacteristicName);
        writer.WriteUIntEnum(Difficulty);

        // Difficulties
        writer.WriteByte((byte)Requirements.Count);

        foreach (var difficulty in Requirements)
        {
            writer.WriteByte((byte)difficulty.Key);
            writer.WriteByte((byte)difficulty.Value.Length);

            foreach (var requirement in difficulty.Value)
                writer.WriteString(requirement);
        }

        // Contributors
        if (Contributors != null)
        {
            writer.WriteByte((byte)Contributors.Length);

            foreach (var contributor in Contributors)
                writer.WriteSerializable(contributor);
        }
        else
        {
            writer.WriteByte(0);
        }
        
        // DifficultyColors
        writer.WriteByte((byte)MapColors.Count);
        
        foreach (var diffColorPair in MapColors)
        {
            writer.WriteByte((byte)diffColorPair.Key);
            writer.WriteSerializable(diffColorPair.Value);
        }
    }

    public override void ReadFrom(ref NetReader reader)
    {
        // Main
        LevelHash = reader.ReadString();
        SongName = reader.ReadString();
        SongSubName = reader.ReadString();
        SongAuthorName = reader.ReadString();
        LevelAuthorName = reader.ReadString();
        BeatsPerMinute = reader.ReadFloat();
        SongDuration = reader.ReadFloat();
        CharacteristicName = reader.ReadString();
        Difficulty = reader.ReadUIntEnum<BeatmapDifficulty>();

        if (reader.EndOfData)
        {
            // Older versions of MultiplayerCore may not send the rest of the data
            Requirements = new();
            Contributors = null;
            MapColors = new();
            return;
        }
        
        // Difficulties
        var difficultyCount = reader.ReadByte();
        Requirements = new Dictionary<BeatmapDifficulty, string[]>(difficultyCount);

        for (var i = 0; i < difficultyCount; i++)
        {
            var difficulty = reader.ReadByteEnum<BeatmapDifficulty>();
            var requirementCount = reader.ReadByte();
            
            var requirements = new string[requirementCount];

            for (var j = 0; j < requirementCount; j++)
                requirements[j] = reader.ReadString();

            Requirements.Add(difficulty, requirements);
        }

        // Contributors
        var contributorCount = reader.ReadByte();
        Contributors = new Contributor[contributorCount];

        for (var i = 0; i < contributorCount; i++)
            Contributors[i] = reader.ReadSerializable<Contributor>();

        // DifficultyColors
        var colorCount = reader.ReadByte();
        MapColors = new Dictionary<BeatmapDifficulty, DifficultyColors>();

        for (var i = 0; i < colorCount; i++)
        {
            var difficulty = reader.ReadUIntEnum<BeatmapDifficulty>();
            var colors = reader.ReadSerializable<DifficultyColors>();
            MapColors.Add(difficulty, colors);
        }
    }
}