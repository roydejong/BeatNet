using System.Reflection;
using System.Runtime.CompilerServices;
using BeatNet.GameServer.Lobby;

namespace BeatNet.GameServer.GameModes;

public static class GameModeFactory
{
    private static readonly Dictionary<string, Type> GameModeTypeMap = new();
    private static bool _didLazyInit = false;
    
    private static void Init()
    {
        var gameModeTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(GameMode)))
            .ToList();

        foreach (var gameModeType in gameModeTypes)
        {
            var instance = (GameMode)RuntimeHelpers.GetUninitializedObject(gameModeType);
            
            if (GameModeTypeMap.ContainsKey(gameModeType.Name))
                throw new InvalidOperationException($"Duplicate game mode ID: {gameModeType.Name}");
            
            GameModeTypeMap[instance.GameModeId] = gameModeType;
        }
    }
    
    private static void LazyInit()
    {
        if (_didLazyInit)
            return;
        
        Init();
        _didLazyInit = true;
    }

    public static GameMode Instantiate(string gameModeId, LobbyHost host)
    {
        LazyInit();

        if (!GameModeTypeMap.TryGetValue(gameModeId, out var gameModeType))
            throw new InvalidOperationException($"Unknown game mode ID: {gameModeId}");

        return (GameMode)Activator.CreateInstance(gameModeType, host)!;
    }
}