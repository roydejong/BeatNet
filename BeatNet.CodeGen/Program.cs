using BeatNet.CodeGen.Analysis;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Generator;

if (args.Length != 2)
{
    Console.WriteLine("Usage: BeatNet.CodeGen <input-dir> <output-dir>");
    Environment.Exit(1);
    return;
}

var dirSrc = args[0];
var dirDst = args[1];

var checkDirs = new string[] { dirSrc, dirDst };
foreach (var dir in checkDirs)
{
    if (!Directory.Exists(dir))
    {
        Console.WriteLine($"Directory does not exist: {dir}");
        Environment.Exit(1);
        return;
    }
}

// ---------------------------------------------------------------------------------------------------------------------

Console.WriteLine("BeatNet.CodeGen");
Console.WriteLine();
Console.WriteLine("Source: " + dirSrc);
Console.WriteLine("Target: " + dirDst);
Console.WriteLine();

var results = new Results();

// ---------------------------------------------------------------------------------------------------------------------

var assemblyAllowList = new string[] { "Main", "BGNetCore" };
var assemblyDirs = Directory.GetDirectories(dirSrc);

foreach (var assemblyDir in assemblyDirs)
{
    var assemblyName = Path.GetFileName(assemblyDir);
    if (!assemblyAllowList.Contains(assemblyName))
        continue;
    
    Console.WriteLine("Processing assembly: " + assemblyName);

    var assemblyDirFull = Path.GetFullPath(assemblyDir);
    var files = Directory.GetFiles(assemblyDirFull, "*.cs", SearchOption.AllDirectories);

    foreach (var file in files)
    {
        var subPath = file[(assemblyDirFull.Length + 1)..];
        
        var fileAnalyzer = new FileAnalyzer(assemblyDirFull, subPath);
        fileAnalyzer.Analyze(results);
    }
}

// ---------------------------------------------------------------------------------------------------------------------

Console.WriteLine($"\nAnalysis complete!");
Console.WriteLine(" • Found RPC Managers: " + results.RpcManagers.Count);
Console.WriteLine(" • Found RPC Definitions: " + results.Rpcs.Count);

// ---------------------------------------------------------------------------------------------------------------------

Console.WriteLine($"\nGenerating output files...");

var gs = new GeneratorSettings()
{
    OutputPath = Path.GetFullPath(dirDst),
    BaseNamespace = "BeatNet.BeatSaber.Generated"
};

foreach (var rpc in results.Rpcs)
{
    var rpcGen = new RpcGenerator(rpc);
    rpcGen.Generate(gs);
}

// ---------------------------------------------------------------------------------------------------------------------

Console.WriteLine($"\nDone. Bye!");