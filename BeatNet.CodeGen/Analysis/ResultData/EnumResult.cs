using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData;

public class EnumResult
{
    public string? ContainingType;
    public string EnumName;
    public string EnumBackingType;
    public Dictionary<int, string> Cases = new();
}