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
        else
            domainAnalyzer = new NetSerializableAnalyzer();

        Console.WriteLine($"Analyzing file {SubPath} [{domainAnalyzer}]...");

        string? baseType = null;
        string? currentType = null;

        for (var pass = 1; pass <= 2; pass++)
        {
            baseType = null;
            currentType = null;

            foreach (var line in File.ReadAllLines(FullPath))
            {
                var lineTrimmed = line.Trim();
                // Skip empty lines
                if (lineTrimmed.Length == 0)
                    continue;
                // Skip comments (dnSpy generates some)
                if (lineTrimmed.StartsWith("//") || lineTrimmed.StartsWith("/*") || lineTrimmed.StartsWith("*/"))
                    continue;

                var lineAnalyzer = new LineAnalyzer(lineTrimmed, currentType);

                if (lineAnalyzer.IsClass || lineAnalyzer.IsStruct)
                {
                    baseType ??= lineAnalyzer.DeclaredName;
                    currentType = lineAnalyzer.DeclaredName;
                    Console.WriteLine($"Processing class: {AssemblyName}.{baseType} › {currentType}");
                }

                if (pass == 1)
                {
                    domainAnalyzer?.AnalyzeLine_FirstPass(lineAnalyzer, results);
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
        var blockList = new string[] { "ConnectedPlayerManager", "PartyMessageHandler", "AuthenticationToken",
            "MultiplayerSessionManager", "NetworkPacketSerializer", "PoolableSerializable" };
        foreach (var block in blockList)
        {
            if (FileNameNoExt.Contains(block))
                return true;
        }
        
        // Explicit Allowlist
        var allowList = new string[] { "Serializable", "SyncState" };
        foreach (var allow in allowList)
        {
            if (FileNameNoExt.Contains(allow))
                return false;
        }
        
        // RPC manager: its private classes hold all the RPC enums and classes
        if (FileNameNoExt.EndsWith("RpcManager") && !FileNameNoExt.StartsWith("I")) // ignore the interfaces (I*)
            return false;

        // NetSerializable, Serializable, SyncStates, etc.
        var readAhead = File.ReadAllText(FullPath);
        if (readAhead.Contains(": INetImmutableSerializable") || readAhead.Contains(": INetSerializable"))
            return false;

        return true;
    }
}