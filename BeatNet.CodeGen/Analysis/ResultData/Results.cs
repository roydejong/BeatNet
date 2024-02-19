namespace BeatNet.CodeGen.Analysis.ResultData;

public class Results
{
    public List<RpcManagerResult> RpcManagers { get; set; } = new();
    public List<RpcResult> Rpcs { get; set; } = new();
    public List<NetSerializableResult> NetSerializables { get; set; } = new();
    public List<EnumResult> Enums { get; set; } = new();
}