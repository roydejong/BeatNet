using System.Diagnostics;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.Domains;

public class RpcManagerAnalyzer : ISubAnalyzer
{
    private string? _rpcManagerName = null;
    private RpcManagerResult? _currentResult = null;
    private Dictionary<string, RpcResult> _namedRpcs = new();

    public void AnalyzeLine_FirstPass(LineAnalyzer line, Results results)
    {
        if (line.IsClass && line.DeclaredName!.Contains("RpcManager"))
        {
            _rpcManagerName = line.DeclaredName;

            _currentResult = new RpcManagerResult { RpcManagerName = _rpcManagerName };
            _currentResult.RpcManagerName = _rpcManagerName;

            results.RpcManagers.Add(_currentResult);
        }

        if (_currentResult == null)
            return;

        if (line.IsClass && line.ClassInheritors?[0].TypeName == "RemoteProcedureCall")
        {
            var rpc = new RpcResult();
            rpc.RpcManagerName = _rpcManagerName!;
            rpc.RpcName = line.DeclaredName!;
            
            var genericParams = line.ClassInheritors[0].Generics;
            if (genericParams != null)
            {
                foreach (var genericParam in genericParams)
                {
                    rpc.Params.Add(new TypedParam()
                    {
                        TypeName = genericParam,
                        Name = $"Unk{rpc.Params.Count + 1}"
                    });
                }
            }
            
            results.Rpcs.Add(rpc);
            _namedRpcs.Add(rpc.RpcName, rpc);
        }
    }

    public void AnalyzeLine_SecondPass(LineAnalyzer line, Results results)
    {
        if (line is { IsMethod: true, IsConstructor: false })
        {
            // Rpc managers will contain methods like "SetGameplaySceneSyncFinished" which would then call the "SetGameplaySceneSyncFinishedRpc" with properly named params, e.g.:
            // public void SetGameplaySceneSyncFinished(PlayerSpecificSettingsAtStartNetSerializable playersAtGameStartNetSerializable, string sessionGameId)
            // We want to get the RPC param names from these methods for RPC class generation

            if (line.DeclaredName!.StartsWith("Invoke"))
                return;
            if (line.DeclaredName is "Dispose" or "EnabledForPlayer")
                return;
            
            var expectedRpcName = line.DeclaredName + "Rpc";
            
            if (_namedRpcs.TryGetValue(expectedRpcName, out var rpc))
            {
                var paramList = line.MethodParams;
                if (paramList != null)
                {
                    for (var i = 0; i < paramList.Count; i++)
                    {
                        var param = paramList[i];
                        var rpcParam = rpc.Params[i];
                        
                        rpcParam.TypeName = param.TypeName;
                        rpcParam.Name = param.Name;
                    }
                }
            }
            else
            {
                Debugger.Break();
            }
        }
    }

    public void Analyze_AfterFile(Results results)
    {
    }
}