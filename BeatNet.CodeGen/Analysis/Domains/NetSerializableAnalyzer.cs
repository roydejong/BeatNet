﻿using System.Diagnostics;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.Domains;

public class NetSerializableAnalyzer : ISubAnalyzer
{
    private string? _typeName = null;
    private NetSerializableResult _currentResult = null;
    
    public void AnalyzeLine_FirstPass(LineAnalyzer line, Results results)
    {
        if ((line.IsClass || line.IsStruct) && _typeName == null)
        {
            _typeName = line.DeclaredName!;
            
            _currentResult = new NetSerializableResult();
            _currentResult.TypeName = _typeName;
            
            results.NetSerializables.Add(_currentResult);
        }

        if (_typeName == null)
            return;

        if (line.IsField)
        {
            var name = line.DeclaredName!;
            name = name.Trim('_');

            if (name.Contains("__BackingField"))
                return;

            if (line.DeclaredType!.Contains("IReadOnly"))
                return;

            // if (line.Const)
            //     return;
            
            _currentResult.Fields[name] = new TypedParam()
            {
                TypeName = line.DeclaredType!,
                ParamName = name
            };
        }
    }

    private bool _inDeserialize = false;
    private int _deserializeDepth = 0;

    private static readonly string[] SpecialTypesIgnore = { "ByteArrayNetSerializable",
        "NodePoseSyncStateDeltaNetSerializable", "PlayerSpecificSettingsAtStartNetSerializable",
        "StandardScoreSyncStateDeltaNetSerializable", "BitMaskArray", "BitMaskSparse",
        "MultiplayerAvatarsData", "PlayersLobbyPermissionConfigurationNetSerializable",
        "PlayersMissingEntitlementsNetSerializable"
    };

    public void AnalyzeLine_SecondPass(LineAnalyzer line, Results results)
    {
        if (_typeName == null || SpecialTypesIgnore.Contains(_typeName))
            return;

        if (!_inDeserialize)
        {
            _deserializeDepth = 0;
            if (line.IsMethod && line.DeclaredName!.Contains("Deserialize"))
                _inDeserialize = true;
            return;
        }

        if (line.IsOpenBracket)
        {
            _deserializeDepth++;
            return;
        } 
        else if (line.IsCloseBracket)
        {
            _deserializeDepth--;
            return;
        }

        if (_deserializeDepth == 0)
        {
            _inDeserialize = false;
            return;
        }

        var rawLine = line.RawLine;
        rawLine = rawLine.Replace("this.", "");

        if (rawLine.Contains("reader."))
        {
            // Reader assign
            var eqIdx = rawLine.IndexOf('=');
            var refField = rawLine[..eqIdx].Trim();
            
            var dotIdx = rawLine.IndexOf("reader.", StringComparison.Ordinal);
            var callType = rawLine[(dotIdx + 7)..];
            
            _currentResult.DeserializeInstructions.Add(new DeserializeInstruction()
            {
                CallType = callType,
                FieldName = refField.Trim('_')
            });
        }
        else if (rawLine.Contains(".Deserialize"))
        {
            // Deserialize assign
            var dotIdx = rawLine.IndexOf(".Deserialize", StringComparison.Ordinal);
            var refField = rawLine[..dotIdx];
            
            _currentResult.DeserializeInstructions.Add(new DeserializeInstruction()
            {
                CallType = "Deserialize",
                FieldName = refField.Trim('_')
            });
        }
        else if (rawLine.EndsWith("= 1f;"))
        {
            // Hardcoded alpha for networked colors
            _currentResult.DeserializeInstructions.Add(new DeserializeInstruction()
            {
                CallType = "1f",
                FieldName = "a"
            });
        }
        else if (rawLine.Contains("@byte &"))
        {
            // Byte shifted flags
            // TODO
        }
        else
        {
            Debugger.Break();
        }
    }
}