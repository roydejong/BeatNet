using BeatNet.GameServer.Lobby;
using Serilog;

namespace BeatNet.GameServer.Chat;

public class ChatManager(LobbyHost host)
{
    private ILogger? _logger;
    
    public void SetLogger(ILogger logger) =>
        _logger = logger.ForContext<ChatManager>();
    
    public void HandleChatMessage(LobbyPlayer player, string message)
    {
        var isCommand = message.StartsWith('/');

        if (!isCommand)
        {
            _logger?.Information("[{PlayerUserName}] chat: {Text}", player.UserName, message);
            return;
        }
        
        _logger?.Information("[{PlayerUserName}] command: {Text}", player.UserName, message);
        player.SendChatMessage("Unknown command");
        
        // TODO Command handlers
    }
}