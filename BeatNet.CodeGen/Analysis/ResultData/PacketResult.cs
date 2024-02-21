using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData;

public class PacketResult : IResultSerializable
{
    public string PacketName;
    public Dictionary<string, TypedParam> Fields = new();
    public List<DeserializeInstruction> DeserializeInstructions = new();

    public string GetSelfName() => PacketName;
    public IEnumerable<TypedParam> GetFields() => Fields.Values;
    public IEnumerable<DeserializeInstruction> GetInstructions() => DeserializeInstructions;
}