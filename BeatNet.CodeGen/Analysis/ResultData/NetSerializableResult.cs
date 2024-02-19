using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData;

public class NetSerializableResult
{
    public string TypeName;
    public List<TypedParam> Fields = new();
}