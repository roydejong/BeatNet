﻿using System.Text;
using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Generator;

public class NetSerializableGenerator
{
    public readonly NetSerializableResult NetSerializable;
    
    public NetSerializableGenerator(NetSerializableResult netSerializable)
    {
        NetSerializable = netSerializable;
    }

    public void Generate(GeneratorSettings gs)
    {
        var targetNamespace = $"{gs.BaseNamespace}.NetSerializable";
        var targetDir = Path.Combine(gs.OutputPath, "NetSerializable");
        
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);
        
        var targetFile = Path.Combine(targetDir, $"{NetSerializable.TypeName}.cs");
        
        using var sw = new StreamWriter(targetFile);
        
        // Header, usings, namespace, class declaration
        sw.WriteLine("// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)");
        sw.WriteLine("// Do not modify manually");
        sw.WriteLine();
        sw.WriteLine("using System;");
        sw.WriteLine();
        sw.WriteLine($"namespace {targetNamespace};");
        sw.WriteLine();
        
        sw.WriteLine($"public sealed class {NetSerializable.TypeName}");
        sw.WriteLine("{");
        
        // RPC Params & Constructor w/ params
        var constructorBuffer = new StringBuilder();
        var constructorBodyBuffer = new StringBuilder();
        
        constructorBuffer.Append($"\tpublic {NetSerializable.TypeName}(");
        
        var paramNo = 0;
        foreach (var field in NetSerializable.Fields)
        {
            sw.WriteLine($"\tpublic {field.TypeName} {field.ParamNameForField} {{ get; set; }}");
            
            if (paramNo > 0)
                constructorBuffer.Append(", ");
            
            constructorBuffer.Append($"{field.TypeName} {field.ParamNameForArg}");
            constructorBodyBuffer.AppendLine($"\t\t{field.ParamNameForField} = {field.ParamNameForArg};");
            
            paramNo++;
        }
        
        sw.WriteLine();
        constructorBuffer.AppendLine(")");
        constructorBuffer.AppendLine($"\t{{");
        constructorBuffer.Append(constructorBodyBuffer);
        constructorBuffer.AppendLine($"\t}}");
        sw.Write(constructorBuffer);
        
        // End of class and file
        sw.WriteLine("}");
        sw.Close();
    }
}