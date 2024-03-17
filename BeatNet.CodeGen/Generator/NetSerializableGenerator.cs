﻿using System.Diagnostics;
using System.Text;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Generator.Util;

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
        if (NetSerializable.Fields.Count == 0)
            return;

        var isMultiplayerSessionPacket = NetSerializable.TypeName.Contains("Packet")
                                         || NetSerializable.TypeName.Contains("SyncStateNet")
                                         || NetSerializable.TypeName.Contains("SyncStateDeltaNet");
        
        var subPath = isMultiplayerSessionPacket ? "MultiplayerSession" : "NetSerializable";
        var baseType = isMultiplayerSessionPacket ? "BaseSessionPacket" : "INetSerializable";
        
        var targetNamespace = $"{gs.BaseNamespace}.{subPath}";
        var targetDir = Path.Combine(gs.OutputPath, subPath);
        
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);
        
        var targetFile = Path.Combine(targetDir, $"{NetSerializable.TypeName}.cs");
        
        using var sw = new StreamWriter(targetFile);
        
        // Header, usings, namespace, class declaration
        sw.WriteLine("// This file was generated by BeatNet.CodeGen (NetSerializableGenerator)");
        sw.WriteLine("// Do not modify manually");
        sw.WriteLine();
        sw.WriteLine("using System;");
        sw.WriteLine("using BeatNet.Lib.Net.Interfaces;");
        sw.WriteLine("using BeatNet.Lib.Net.IO;");
        sw.WriteLine("using BeatNet.Lib.BeatSaber.Common;");
        sw.WriteLine("using BeatNet.Lib.BeatSaber.Generated.Enum;");
        if (isMultiplayerSessionPacket)
            sw.WriteLine("using BeatNet.Lib.BeatSaber.Generated.NetSerializable;");
        sw.WriteLine();
        sw.WriteLine($"namespace {targetNamespace};");
        sw.WriteLine();
        sw.WriteLine($"// ReSharper disable InconsistentNaming IdentifierTypo ClassNeverInstantiated.Global MemberCanBePrivate.Global");
        sw.WriteLine($"public sealed class {NetSerializable.TypeName} : {baseType}");
        sw.WriteLine("{");

        if (isMultiplayerSessionPacket)
        {
            var messageTypeType = "SessionMessageType";
            var packetTypeCase = NetSerializable.TypeName
                .Replace("Packet", "")
                .Replace("NetSerializable", "")
                .Replace("Standard", "");
            if (weirdPacketCaseMap.TryGetValue(packetTypeCase, out var packetTypeCaseMapped))
                packetTypeCase = packetTypeCaseMapped;
            sw.WriteLine("\t" + $"public override {messageTypeType} SessionMessageType => {messageTypeType}.{packetTypeCase};");
            sw.WriteLine();
        }
        
        sw.WriteLine(
            FieldGenerator.GenerateFields(NetSerializable)
        );
        
        // BitMasks: "MinValue" / "MaxValue" util
        if (NetSerializable.TypeName.StartsWith("BitMask"))
        {
            var bitCount = int.Parse(NetSerializable.TypeName.Substring("BitMask".Length));
            var ulongCount = bitCount / 64;
            
            sw.Write($"\tpublic static {NetSerializable.TypeName} MinValue => new(");
            for (var i = 0; i < ulongCount; i++)
            {
                sw.Write("0");
                if (i < ulongCount - 1)
                    sw.Write(", ");
            }
            sw.WriteLine(");");
            
            sw.Write($"\tpublic static {NetSerializable.TypeName} MaxValue => new(");
            for (var i = 0; i < ulongCount; i++)
            {
                sw.Write("ulong.MaxValue");
                if (i < ulongCount - 1)
                    sw.Write(", ");
            }
            sw.WriteLine(");");
            
            sw.WriteLine();
        }
        
        sw.WriteLine(
            ConstructorGenerator.GenerateConstructor(NetSerializable)
        );
        
        sw.Write(
            ReadWriteMethodGenerator.GenerateMethods(NetSerializable, overrideKeyword: (baseType != "INetSerializable"))
        );
        sw.Write("}");
        sw.Close();
    }

    private static readonly Dictionary<string, string> weirdPacketCaseMap = new()
    {
    };
}