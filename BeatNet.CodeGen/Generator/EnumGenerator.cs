﻿using System.Diagnostics;
using System.Text;
using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Generator;

public class EnumGenerator
{
    public readonly EnumResult Enum;
    
    public EnumGenerator(EnumResult enumResult)
    {
        Enum = enumResult;
    }

    public void Generate(GeneratorSettings gs)
    {
        if (Enum.EnumName == "RpcType")
        {
            var managerNamePlain = Enum.ContainingType!.Replace("RpcManager", "");
            Enum.EnumName = $"{managerNamePlain}RpcType";
        }
        else if (Enum is { EnumName: "MessageType", ContainingType: "ConnectedPlayerManager" })
        {
            // Ignore this enum, it's not used
            return;
        }
        
        var targetNamespace = $"{gs.BaseNamespace}.Enum";
        var targetDir = Path.Combine(gs.OutputPath, "Enum");
        
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);
        
        var targetFile = Path.Combine(targetDir, $"{Enum.EnumName}.cs");
        
        using var sw = new StreamWriter(targetFile);
        
        // Header, usings, namespace, class declaration
        sw.WriteLine("// This file was generated by BeatNet.CodeGen (EnumGenerator)");
        sw.WriteLine("// Do not modify manually");
        sw.WriteLine();
        sw.WriteLine($"namespace {targetNamespace};");
        sw.WriteLine();
        
        if (Enum.ContainingType != null)
            sw.WriteLine($"// Context: {Enum.ContainingType}\n");
        
        if (Enum.Flags)
            sw.WriteLine("[Flags]");
        
        sw.WriteLine($"public enum {Enum.EnumName} : {Enum.EnumBackingType}");
        sw.WriteLine("{");

        var firstEnum = true;
        foreach (var enumCase in Enum.Cases)
        {
            if (firstEnum)
                firstEnum = false;
            else
                sw.WriteLine(',');
            sw.Write("\t");
            sw.Write($"{enumCase.Value} = {enumCase.Key}");
        }
        sw.WriteLine();
        
        // End of class and file
        sw.Write("}");
        sw.Close();
    }
}