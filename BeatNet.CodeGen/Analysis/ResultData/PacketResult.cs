using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData;

public class PacketResult
{
    public string PacketName;
    public Dictionary<string, TypedParam> Fields = new();
}