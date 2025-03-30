using System.Net;
using BeatNet.GameServer.BSSB;
using BeatNet.GameServer.BSSB.Models;
using BeatNet.GameServer.BSSB.Models.Enums;
using BeatNet.GameServer.GameModes;
using BeatNet.Lib;

namespace BeatNet.GameServer.Lobby;

public partial class LobbyHost
{
    public const int BssbAnnounceIntervalSeconds = 5;

    private readonly BssbClient _bssbClient = new();
    private string? _bssbActiveKey;

    private bool _announceNeeded;
    private long _lastAnnounce;

    private void _UpdateServerBrowser()
    {
        if (!_announceNeeded)
        {
            var timeSinceAnnounce = SyncTime - _lastAnnounce;
            if (timeSinceAnnounce > BssbAnnounceIntervalSeconds * 1000)
                _announceNeeded = true;
        }

        if (!_announceNeeded)
            return;

        _announceNeeded = false;
        _lastAnnounce = SyncTime;

        _ = PerformBssbAnnounce();
    }

    public async Task PerformBssbAnnounce(bool forceRemove = false)
    {
        var shouldAnnounce = IsRunning && IsPublic && !forceRemove;

        if (!shouldAnnounce)
        {
            // We are not in a state where we should announce; remove if needed, then bail
            if (_bssbActiveKey != null)
            {
                var removeResult = await _bssbClient.UnAnnounce(new UnAnnounceParams()
                {
                    HostSecret = ServerUserId,
                    HostUserId = ServerUserId,
                    SelfUserId = ServerUserId
                });

                if (removeResult?.Result != null)
                    _logger?.Information("Removed server browser listing: {Result}", removeResult.Result);

                _bssbActiveKey = null;
            }

            return;
        }

        if (WanAddress == null)
            throw new InvalidOperationException("Cannot announce to server browser without WAN address");

        // Generate and send announce
        var announce = GenerateAnnounce();
        var result = await _bssbClient.Announce(announce);

        if (result?.Success ?? false)
        {
            if (_bssbActiveKey == null)
            {
                _logger?.Information("Successfully announced to server browser: {Url}",
                    $"{BssbClient.BaseUrl}/game/{result.Key}");
            }

            _bssbActiveKey = result.Key;
        }
        else
        {
            _logger?.Warning("Server browser announce failed: {Error}",
                result?.ServerMessage ?? "Communication error");
        }
    }

    private BssbServer GenerateAnnounce()
    {
        var allPlayers = _players.Values.Where(p => p.SortIndex.HasValue).ToList();

        var truePlayerCount = allPlayers.Count;
        var playerList = new List<BssbServerPlayer>(truePlayerCount + 1);

        // Virtual host player
        playerList.Add(new BssbServerPlayer()
        {
            UserId = ServerUserId,
            UserName = "BeatNet",
            PlatformType = "steam",
            PlatformUserId = null,
            SortIndex = -1,
            IsMe = true,
            IsHost = true,
            IsPartyLeader = true,
            IsAnnouncing = true,
            CurrentLatency = 0
        });

        // Real players
        foreach (var player in allPlayers)
        {
            playerList.Add(new BssbServerPlayer()
            {
                UserId = player.UserId,
                UserName = player.UserName,
                PlatformType = player.Platform.ToString(),
                PlatformUserId = player.PlatformUserId,
                SortIndex = player.SortIndex!.Value,
                IsMe = false,
                IsHost = false,
                IsPartyLeader = false,
                IsAnnouncing = false,
                CurrentLatency = player.LatencyAverage.CurrentAverage / 1000f
            });
        }

        // Current level data
        BssbServerLevel? levelData = null;

        var currentLevel = GameMode.CurrentLevel;
        var currentModifiers = GameMode.CurrentModifiers;
        var sessionId = GameMode.GameplaySessionId;

        if (currentLevel != null)
        {
            levelData = new BssbServerLevel()
            {
                LevelId = currentLevel.LevelId,
                Characteristic = currentLevel.Characteristic,
                Modifiers = currentModifiers,
                Difficulty = currentLevel.Difficulty,
                SessionGameId = sessionId,
                SongName = currentLevel.SongName,
                SongSubName = currentLevel.SongSubName,
                SongAuthorName = currentLevel.SongAuthorName,
            };
        }

        // Combined server data
        return new BssbServer()
        {
            RemoteUserId = ServerUserId,
            RemoteUserName = "BeatNet",
            HostSecret = ServerUserId,
            ManagerId = ServerUserId,
            PlayerCount = truePlayerCount,
            PlayerLimit = MaxPlayerCount,
            GameplayMode = GameMode.GameplayServerMode,
            Name = ServerName,
            LobbyState = GameMode.LobbyState,
            Level = levelData,
            LobbyDifficulty = BssbDifficulty.All,
            LevelDifficulty = BssbDifficulty.All,
            ServerTypeCode = GameMode is QuickPlayGameMode ? "beatnet_quickplay" : "beatnet_custom",
            EndPoint = new DnsEndPoint(WanAddress!.ToString(), PortNumber),
            GameVersion = VersionConsts.GameVersionLabel,
            MultiplayerCoreVersion = VersionConsts.MultiplayerCoreVersion,
            MultiplayerExtensionsVersion = null,
            ServerTypeText = "BeatNet",
            EncryptionMode = "none",
            Players = playerList
        };
    }
}