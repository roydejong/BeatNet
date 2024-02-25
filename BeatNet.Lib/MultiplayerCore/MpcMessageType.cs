namespace BeatNet.Lib.MultiplayerCore;

public enum MpcMessageType : byte
{
    // MultiplayerCore
    
    /// <summary>
    /// https://github.com/Goobwabber/MultiplayerCore/blob/main/MultiplayerCore/Beatmaps/Packets/MpBeatmapPacket.cs
    /// </summary>
    MpBeatmapPacket,
    /// <summary>
    /// https://github.com/Goobwabber/MultiplayerCore/blob/main/MultiplayerCore/Players/MpPlayerData.cs
    /// </summary>
    MpPlayerData,
    
    // MultiplayerChat
    
    /// <summary>
    /// https://github.com/roydejong/BeatSaberMultiplayerChat/blob/main/Network/MpcTextChatPacket.cs
    /// </summary>
    MpcTextChatPacket,
    
    /// <summary>
    /// Fallback for message types not explicitly supported by BeatNet.
    /// </summary>
    Generic = 255
}

public static class MpcMessageTypeExtensions
{
    public static readonly Dictionary<string, MpcMessageType> TranslationTable = new()
    {
        // MultiplayerCore
        {"MpBeatmapPacket", MpcMessageType.MpBeatmapPacket},
        {"MpPlayerData", MpcMessageType.MpPlayerData},
        
        // MultiplayerChat
        {"MpcTextChatPacket", MpcMessageType.MpcTextChatPacket}
    };

    public static string? GetMpCorePacketName(this MpcMessageType value) =>
        TranslationTable.FirstOrDefault(kv => kv.Value == value).Key ?? null;

    public static MpcMessageType GetMpCoreMessageType(string mpCorePacketName) =>
        TranslationTable.GetValueOrDefault(mpCorePacketName, MpcMessageType.Generic);
}