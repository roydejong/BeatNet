namespace BeatNet.CodeGen.Analysis.ResultData;

public class Results
{
    public List<RpcManagerResult> RpcManagers { get; set; } = new();
    public List<RpcResult> Rpcs { get; set; } = new();
}