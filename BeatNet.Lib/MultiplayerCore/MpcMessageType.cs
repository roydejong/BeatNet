namespace BeatNet.Lib.MultiplayerCore;

public enum MpcMessageType : byte
{
    // -----------------------------------------------------------------------------------------------------------------
    // MultiplayerCore 
    
    /// <summary>
    /// https://github.com/Goobwabber/MultiplayerCore/blob/main/MultiplayerCore/Beatmaps/Packets/MpBeatmapPacket.cs
    /// </summary>
    MpBeatmapPacket,
    /// <summary>
    /// https://github.com/Goobwabber/MultiplayerCore/blob/main/MultiplayerCore/Players/MpPlayerData.cs
    /// </summary>
    MpPlayerData,
    /// <summary>
    /// https://github.com/Goobwabber/MultiplayerCore/blob/main/MultiplayerCore/Players/Packets/GetMpPerPlayerPacket.cs
    /// </summary>
    GetMpPerPlayerPacket,
    /// <summary>
    /// https://github.com/Goobwabber/MultiplayerCore/blob/main/MultiplayerCore/Players/Packets/MpPerPlayerPacket.cs
    /// </summary>
    MpPerPlayerPacket,
    
    // -----------------------------------------------------------------------------------------------------------------
    // MultiplayerChat
    
    /// <summary>
    /// https://github.com/roydejong/BeatSaberMultiplayerChat/blob/main/Network/MpcTextChatPacket.cs
    /// </summary>
    MpcTextChatPacket,
    
    // -----------------------------------------------------------------------------------------------------------------
    // Generic
    
    /// <summary>
    /// Fallback for message types not explicitly supported by BeatNet.
    /// Any packet name that is not recognized will use this type.
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
        {"GetMpPerPlayerPacket", MpcMessageType.GetMpPerPlayerPacket},
        {"MpPerPlayerPacket", MpcMessageType.MpPerPlayerPacket},
        
        // MultiplayerChat
        {"MpcTextChatPacket", MpcMessageType.MpcTextChatPacket}
    };

    public static string? GetMpCorePacketName(this MpcMessageType value) =>
        TranslationTable.FirstOrDefault(kv => kv.Value == value).Key ?? null;

    public static MpcMessageType GetMpCoreMessageType(string mpCorePacketName) =>
        TranslationTable.GetValueOrDefault(mpCorePacketName, MpcMessageType.Generic);
}