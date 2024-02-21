using System.Diagnostics;
using BeatNet.CodeGen.Analysis.Domains;
using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Analysis;

public class FileAnalyzer
{
    public readonly string AssemblyName;
    public readonly string BaseDir;
    public readonly string SubPath;
    public readonly string FullPath;
    public readonly string FileName;
    public readonly string FileNameNoExt;

    public FileAnalyzer(string baseDir, string subPath)
    {
        AssemblyName = Path.GetFileNameWithoutExtension(baseDir);
        BaseDir = baseDir;
        SubPath = subPath;
        FullPath = Path.Combine(BaseDir, SubPath);
        FileName = Path.GetFileName(FullPath);
        FileNameNoExt = Path.GetFileNameWithoutExtension(FullPath);
    }

    public void Analyze(Results results)
    {
        if (ShouldIgnoreFileName())
            return;

        ISubAnalyzer? domainAnalyzer = null;
        if (SubPath.Contains("RpcManager"))
            domainAnalyzer = new RpcManagerAnalyzer();
        else if (FileNameNoExt == "ConnectedPlayerManager")
            domainAnalyzer = new ConnectedPlayerManagerAnalyzer();
        else if (FileNameNoExt is "SliderData" or "MultiplayerSessionManager" or "NoteData" or "GameplayServerConfiguration")
            domainAnalyzer = null; // Enum extraction only
        else
            domainAnalyzer = new NetSerializableAnalyzer();

        string? baseType = null;
        string? currentType = null;

        for (var pass = 1; pass <= 2; pass++)
        {
            baseType = null;
            currentType = null;
            EnumResult? currentEnum = null;
            int enumValueGen = 0;

            foreach (var line in File.ReadAllLines(FullPath))
            {
                var lineTrimmed = line.Trim();
                // Skip empty lines
                if (lineTrimmed.Length == 0)
                    continue;
                // Skip comments (dnSpy generates some)
                if (lineTrimmed.StartsWith("//") || lineTrimmed.StartsWith("/*") || lineTrimmed.StartsWith("*/"))
                    continue;

                var lineAnalyzer = new LineAnalyzer(lineTrimmed, currentType, currentEnum != null);

                if (lineAnalyzer.IsClass || lineAnalyzer.IsStruct)
                {
                    baseType ??= lineAnalyzer.DeclaredName;
                    currentType = lineAnalyzer.DeclaredName;
                }

                if (pass == 1)
                {
                    domainAnalyzer?.AnalyzeLine_FirstPass(lineAnalyzer, results);

                    if (lineAnalyzer.IsEnum)
                    {
                        var enumName = lineAnalyzer.DeclaredName;
                        
                        if (enumName == "Type")
                            enumName = "SliderType";
                        
                        currentEnum = new EnumResult
                        {
                            ContainingType = baseType,
                            EnumName = enumName,
                            EnumBackingType = lineAnalyzer.DeclaredType ?? "int"
                        };
                        results.Enums.Add(currentEnum);
                    }
                    else if (currentEnum != null)
                    {
                        if (lineAnalyzer.IsEnumCase)
                        {
                            int enumValue = enumValueGen;

                            if (lineAnalyzer.DefaultValue != null)
                            {
                                int.TryParse(lineAnalyzer.DefaultValue, out enumValue);
                            }

                            if (enumValue > enumValueGen)
                                enumValueGen = enumValue;

                            currentEnum.Cases.Add(enumValue, lineAnalyzer.DeclaredName!);
                            enumValueGen++;
                        }
                        else if (lineAnalyzer.IsCloseBracket)
                        {
                            currentEnum = null;
                            enumValueGen = 0;
                        }
                        else if (lineAnalyzer.IsOpenBracket)
                        {
                            // Continue
                        }
                        else
                        {
                            Debugger.Break();
                        }
                    }
                }
                else if (pass == 2)
                {
                    domainAnalyzer?.AnalyzeLine_SecondPass(lineAnalyzer, results);
                }
            }
        }
    }

    private bool ShouldIgnoreFileName()
    {
        // Explicit Blocklist
        var blockList = new string[] { "PartyMessageHandler", "AuthenticationToken", "ByteArrayNetSerializable",
            "NetworkPacketSerializer", "PoolableSerializable", "StateBuffer", "BitMaskArray", "BitMaskSparse",
            "RemoteProcedureCall", "Extensions", "GameplayType", "ScoringType" };
        foreach (var block in blockList)
        {
            if (FileNameNoExt.Contains(block))
                return true;
        }
        
        // Explicit Allowlist
        var allowList = new string[] { "Serializable", "SyncState", "EntitlementsStatus", "CannotStartGameReason",
            "MultiplayerGameState", "ColorType", "NoteCutDirection", "NoteLineLayer", "SliderMidAnchorMode",
            "NoteData", "DiscoveryPolicy", "InvitePolicy", "GameplayServerMode", "SongSelectionMode",
            "GameplayServerControlSettings", "MultiplayerAvatarData", "SliderData", "MultiplayerSessionManager"
        };
        foreach (var allow in allowList)
        {
            if (FileNameNoExt.Contains(allow))
                return false;
        }
        var allowListFullName = new string[] { "RankModel", "BeatmapDifficulty", "ConnectedPlayerManager", 
            "DisconnectedReason" };
        foreach (var allow in allowListFullName)
        {
            if (FileNameNoExt == allow)
                return false;
        }
        
        // RPC manager: its private classes hold all the RPC enums and classes
        if (FileNameNoExt.EndsWith("RpcManager") && !FileNameNoExt.StartsWith("I")) // ignore the interfaces (I*)
            return false;

        // NetSerializable, Serializable, SyncStates, etc.
        var readAhead = File.ReadAllText(FullPath);
        if (readAhead.Contains("INetImmutableSerializable") || readAhead.Contains("INetSerializable"))
            return false;

        return true;
    }
}