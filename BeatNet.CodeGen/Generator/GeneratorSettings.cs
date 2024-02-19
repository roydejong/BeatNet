using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Generator;

public class GeneratorSettings
{
    public string OutputPath { get; set; }
    public string BaseNamespace { get; set; }
    public Results Results { get; set; }
}