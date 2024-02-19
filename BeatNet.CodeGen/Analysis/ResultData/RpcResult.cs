namespace BeatNet.CodeGen.Analysis.ResultData;

public class RpcResult
{
    public string RpcManagerName { get; set; }
    public string RpcName { get; set; }
    public List<string> ParamTypes { get; set; } = new();
}