using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Analysis;

public class RpcManagerAnalyzer : ISubAnalyzer
{
    private string? _rpcManagerName = null;
    private RpcManagerResult? _currentResult = null;

    public void AnalyzeLine(LineAnalyzer line, Results results)
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
            
            Console.Write($" • Found RPC definition: {_rpcManagerName} › {rpc.RpcName}");
            
            var genericParams = line.ClassInheritors[0].Generics;
            if (genericParams != null)
            {
                var firstParam = true;
                Console.Write(" (");
                foreach (var genericParam in genericParams)
                {
                    if (!firstParam)
                        Console.Write(", ");
                    else
                        firstParam = false;
                    
                    Console.Write(genericParam);
                    
                    rpc.ParamTypes.Add(genericParam);
                }
                Console.Write(")");
            }
            
            Console.WriteLine();
            
            results.Rpcs.Add(rpc);
        }
    }
}