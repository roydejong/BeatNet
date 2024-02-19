using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData;

public class RpcResult
{
    public string RpcManagerName { get; set; }
    public string RpcName { get; set; }
    public List<MethodParam> Params { get; set; } = new();
}