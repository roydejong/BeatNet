using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData;

public class NetSerializableResult
{
    public string TypeName;
    public Dictionary<string, TypedParam> Fields = new();
    public List<DeserializeInstruction> DeserializeInstructions = new();
}