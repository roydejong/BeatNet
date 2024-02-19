﻿using System.Text;
using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Generator;

public class RpcGenerator
{
    public readonly RpcResult Rpc;
    
    public RpcGenerator(RpcResult rpc)
    {
        Rpc = rpc;
    }

    public void Generate(GeneratorSettings gs)
    {
        var managerNamePlain = Rpc.RpcManagerName.Replace("RpcManager", "");
        var targetNamespace = $"{gs.BaseNamespace}.Rpc.{managerNamePlain}";
        var targetDir = Path.Combine(gs.OutputPath, "Rpc", managerNamePlain);
        
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);
        
        var targetFile = Path.Combine(targetDir, $"{Rpc.RpcName}.cs");
        
        using var sw = new StreamWriter(targetFile);
        
        // Header, usings, namespace, class declaration
        sw.WriteLine("// This file was generated by BeatNet.CodeGen (RpcGenerator) - Do not modify manually");
        sw.WriteLine("using System;");
        sw.WriteLine("using BeatNet.Lib.BeatSaber.Rpc;");
        sw.WriteLine("using BeatNet.Lib.BeatSaber.Generated.NetSerializable;");
        sw.WriteLine();
        sw.WriteLine($"namespace {targetNamespace};");
        sw.WriteLine();
        sw.WriteLine($"public sealed class {Rpc.RpcName} : BaseRpc");
        sw.WriteLine("{");
        
        if (Rpc.Params.Count > 0)
        {
            // RPC Params & Constructor w/ params
            var constructorBuffer = new StringBuilder();
            constructorBuffer.Append($"\tpublic {Rpc.RpcName}(");
            
            var constructorBodyBuffer = new StringBuilder();
            
            var paramNo = 0;
            
            foreach (var param in Rpc.Params)
            {
                sw.WriteLine($"\tpublic {param.TypeName} {param.ParamNameForField} {{ get; set; }} = null;");
                
                if (paramNo > 0)
                    constructorBuffer.Append(", ");
                
                constructorBuffer.Append($"{param.TypeName} {param.ParamNameForArg} = null");
                
                constructorBodyBuffer.AppendLine($"\t\t{param.ParamNameForField} = {param.ParamNameForArg};");
                
                paramNo++;
            }
            
            sw.WriteLine("\t");

            constructorBuffer.AppendLine(") : base()");
            constructorBuffer.AppendLine($"\t{{");
            constructorBuffer.Append(constructorBodyBuffer);
            constructorBuffer.AppendLine($"\t}}");
            sw.Write(constructorBuffer);
        }
        else
        {
            // Simple constructor
            sw.WriteLine($"\tpublic {Rpc.RpcName}() : base()");
            sw.WriteLine($"\t{{");
            sw.WriteLine("\t\t// RPC without parameters");
            sw.WriteLine($"\t}}");
        }
        
        // End of class and file
        sw.WriteLine("}");
        sw.Close();
    }
}