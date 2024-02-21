﻿using System.Text;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Utils;

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

        var hasParams = Rpc.Params.Count > 0;
        
        // Header, usings, namespace, class declaration
        sw.WriteLine("// This file was generated by BeatNet.CodeGen (RpcGenerator)");
        sw.WriteLine("// Do not modify manually");
        sw.WriteLine();
        if (hasParams)
        {
            sw.WriteLine("using BeatNet.Lib.Net.IO;");
            sw.WriteLine("using BeatNet.Lib.BeatSaber.Rpc;");
            sw.WriteLine("using BeatNet.Lib.BeatSaber.Generated.Enum;");
            sw.WriteLine("using BeatNet.Lib.BeatSaber.Generated.NetSerializable;");
        }
        else
        {
            sw.WriteLine("using BeatNet.Lib.BeatSaber.Rpc;");
            sw.WriteLine("using BeatNet.Lib.BeatSaber.Generated.Enum;");
        }
        sw.WriteLine();
        sw.WriteLine($"namespace {targetNamespace};");
        sw.WriteLine();
        sw.WriteLine($"// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global");
        sw.WriteLine();
        
        var baseType = "BaseRpc";
        
        sw.WriteLine($"public sealed class {Rpc.RpcName} : {baseType}");
        sw.WriteLine("{");
        
        var rpcTypeCase = Rpc.RpcName.Replace("Rpc", "");
        if (weirdRpcCaseMap.TryGetValue(rpcTypeCase, out var rpcTypeCaseMapped))
            rpcTypeCase = rpcTypeCaseMapped;
        
        sw.WriteLine($"\tpublic override byte RpcType => (byte){managerNamePlain}RpcType.{rpcTypeCase};");
        sw.WriteLine();
        
        if (hasParams)
        {
            var writeCodeBuffer = new StringBuilder();
            writeCodeBuffer.AppendLine("\tpublic override void WriteTo(ref NetWriter writer)");
            writeCodeBuffer.AppendLine("\t{");
            writeCodeBuffer.AppendLine("\t\tbase.WriteTo(ref writer);");
            writeCodeBuffer.AppendLine();
            writeCodeBuffer.AppendLine("\t\tvar nullFlags = (byte)(");
            
            for (var paramNo = 0; paramNo < Rpc.Params.Count; paramNo++)
            {
                var param = Rpc.Params[paramNo];
                
                var byteShift = 1 << paramNo;
                
                writeCodeBuffer.Append($"\t\t\t({param.NameForField} != null ? {byteShift} : 0)");
                
                if (paramNo < (Rpc.Params.Count - 1))
                {
                    writeCodeBuffer.AppendLine(" | ");
                }
                else
                {
                    writeCodeBuffer.AppendLine();
                    writeCodeBuffer.AppendLine("\t\t);");
                }
            }

            writeCodeBuffer.AppendLine();
            writeCodeBuffer.AppendLine("\t\twriter.WriteByte(nullFlags);");

            var readCodeBuffer = new StringBuilder();
            readCodeBuffer.AppendLine("\tpublic override void ReadFrom(ref NetReader reader)");
            readCodeBuffer.AppendLine("\t{");
            readCodeBuffer.AppendLine("\t\tbase.ReadFrom(ref reader);");
            readCodeBuffer.AppendLine();
            readCodeBuffer.AppendLine("\t\tvar nullFlags = reader.ReadByte();");
            
            for (var paramNo = 0; paramNo < Rpc.Params.Count; paramNo++)
            {
                var param = Rpc.Params[paramNo];

                var isPrimitiveType = TypeName.IsPrimitive(param.TypeName) && param.TypeName != "string";
                var isEnumType = !isPrimitiveType && gs.Results.Enums.Any(e => e.EnumName == param.TypeName);
                
                var rwFunction = $"Serializable<{param.TypeName}>";
                var writeValueSuffix = (isPrimitiveType || isEnumType) ? ".Value" : "";
                
                if (isEnumType)
                {
                    rwFunction = $"Enum<{param.TypeName}>";
                }
                else
                {
                    switch (param.TypeName)
                    {
                        case "int":
                            rwFunction = "VarInt";
                            break;
                        case "long":
                            rwFunction = "VarLong";
                            break;
                        case "float":
                            rwFunction = "Float";
                            break;
                        case "byte":
                            rwFunction = "Byte";
                            break;
                        case "bool":
                            rwFunction = "Bool";
                            break;
                        case "string":
                            rwFunction = "String";
                            break;
                        case "int[]":
                            // Never seen / tested / implemented in the wild
                            rwFunction = "IntArray";
                            break;
                        case "long[]":
                            // Never seen / tested / implemented in the wild
                            rwFunction = "LongArray";
                            break;
                        case "float[]":
                            // Never seen / tested / implemented in the wild
                            rwFunction = "FloatArray";
                            break;
                        case "byte[]":
                            rwFunction = "ByteArray";
                            break;
                        case "bool[]":
                            // Never seen / tested / implemented in the wild
                            rwFunction = "BoolArray";
                            break;
                        case "Vector3":
                            rwFunction = "Vector3";
                            break;
                        case "Vector4":
                            rwFunction = "Vector4";
                            break;
                        case "Quaternion":
                            rwFunction = "Quaternion";
                            break;
                    }
                }

                writeCodeBuffer.AppendLine();
                writeCodeBuffer.AppendLine($"\t\tif ({param.NameForField} != null)");
                writeCodeBuffer.AppendLine($"\t\t\twriter.Write{rwFunction}({param.NameForField}{writeValueSuffix});");
                
                readCodeBuffer.AppendLine();
                readCodeBuffer.AppendLine($"\t\tif ((nullFlags & (1 << {paramNo})) != 0)");
                readCodeBuffer.AppendLine($"\t\t\t{param.NameForField} = reader.Read{rwFunction}();");
                readCodeBuffer.AppendLine("\t\telse");
                readCodeBuffer.AppendLine($"\t\t\t{param.NameForField} = null;");
            }
            
            var constructorBuffer = new StringBuilder($"\tpublic {Rpc.RpcName}(");
            var constructorBodyBuffer = new StringBuilder();

            for (var paramNo = 0; paramNo < Rpc.Params.Count; paramNo++)
            {
                var param = Rpc.Params[paramNo];
                sw.WriteLine($"\tpublic {param.TypeName}? {param.NameForField} {{ get; set; }} = null;");

                if (paramNo > 0)
                {
                    constructorBuffer.Append(", ");
                }

                constructorBuffer.Append($"{param.TypeName}? {param.NameForArg} = null");
                constructorBodyBuffer.AppendLine($"\t\t{param.NameForField} = {param.NameForArg};");
            }
            
            sw.WriteLine();
            constructorBuffer.AppendLine(")");
            constructorBuffer.AppendLine($"\t{{");
            constructorBuffer.Append(constructorBodyBuffer);
            constructorBuffer.AppendLine($"\t}}");
            sw.WriteLine(constructorBuffer);
            
            writeCodeBuffer.AppendLine("\t}");
            sw.WriteLine(writeCodeBuffer);
            
            readCodeBuffer.AppendLine("\t}");
            sw.Write(readCodeBuffer);
        }
        else
        {
            // Simple constructor
            sw.WriteLine($"\tpublic {Rpc.RpcName}()");
            sw.WriteLine($"\t{{");
            sw.WriteLine("\t\t// RPC without parameters");
            sw.WriteLine($"\t}}");
        }
        
        // End of class and file
        sw.Write("}");
        sw.Close();
    }

    private static readonly Dictionary<string, string> weirdRpcCaseMap = new()
    {
        { "GetPlayersPermissionConfiguration", "GetPermissionConfiguration" },
        { "SetPlayersPermissionConfiguration", "SetPermissionConfiguration" },
        { "SetGameplaySceneSyncFinished", "SetGameplaySceneSyncFinish" },
        { "SetPlayerDidConnectLate", "SetActivePlayerFailedToConnect" }
    };
}