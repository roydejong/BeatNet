using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData;

public class RpcResult : IResultWithFields
{
    public string RpcManagerName { get; set; }
    public string RpcName { get; set; }
    public List<TypedParam> Params { get; set; } = new();

    public string GetSelfName() => RpcName;
    public IEnumerable<TypedParam> GetFields() => Params;
}