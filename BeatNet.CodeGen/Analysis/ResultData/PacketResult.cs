using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData;

public class PacketResult : ISerializeDeserialize
{
    public string PacketName;
    public Dictionary<string, TypedParam> Fields = new();
    public List<DeserializeInstruction> DeserializeInstructions = new();

    public IEnumerable<TypedParam> GetFields() => Fields.Values;
    public IEnumerable<DeserializeInstruction> GetInstructions() => DeserializeInstructions;
}