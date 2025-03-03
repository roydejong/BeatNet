namespace BeatNet.Lib.MultiplayerCore;

public enum MpCoreMessageType : byte
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
    public static readonly Dictionary<string, MpCoreMessageType> TranslationTable = new()
    {
        // MultiplayerCore
        {"MpBeatmapPacket", MpCoreMessageType.MpBeatmapPacket},
        {"MpPlayerData", MpCoreMessageType.MpPlayerData},
        {"GetMpPerPlayerPacket", MpCoreMessageType.GetMpPerPlayerPacket},
        {"MpPerPlayerPacket", MpCoreMessageType.MpPerPlayerPacket},
        
        // MultiplayerChat
        {"MpcTextChatPacket", MpCoreMessageType.MpcTextChatPacket}
    };

    public static string? GetMpCorePacketName(this MpCoreMessageType value) =>
        TranslationTable.FirstOrDefault(kv => kv.Value == value).Key ?? null;

    public static MpCoreMessageType GetMpCoreMessageType(string mpCorePacketName) =>
        TranslationTable.GetValueOrDefault(mpCorePacketName, MpCoreMessageType.Generic);
}