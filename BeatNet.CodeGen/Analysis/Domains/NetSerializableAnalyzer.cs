﻿using System.Diagnostics;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Analysis.Structs;
using BeatNet.CodeGen.Analysis.Util;

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

            if (_typeName is "ColorSerializable" or "Color32Serializable" or "ColorNoAlphaSerializable")
            {
                // Special case: We don't use Unity colors, so these should not be "wrappers" as they are in the game
                // We'll explicitly set up the R/G/B/A fields
                var colorPartType = _typeName == "Color32Serializable" ? "byte" : "float";
                _currentResult.Fields.Add("r", new TypedParam() { ParamName = "r", TypeName = colorPartType });
                _currentResult.Fields.Add("g", new TypedParam() { ParamName = "g", TypeName = colorPartType });
                _currentResult.Fields.Add("b", new TypedParam() { ParamName = "b", TypeName = colorPartType });
                _currentResult.Fields.Add("a", new TypedParam() { ParamName = "a", TypeName = colorPartType });
            }

            return;
        }

        if (_typeName is null or "ColorSerializable" or "Color32Serializable" or "ColorNoAlphaSerializable")
            return;

        if (line.IsField)
        {
            var name = line.DeclaredName!;
            var type = line.DeclaredType!;
            
            name = name.Trim('_');

            if (name.Contains("__BackingField"))
                return;

            if (type.Contains("IReadOnly"))
                return;

            if (name == "sliderType")
                type = "SliderType"; // BG called this "Type" which, well, isn't ideal - so explicit fix

            // if (line.Const)
            //     return;
            
            _currentResult.Fields[name] = new TypedParam()
            {
                TypeName = type,
                ParamName = name
            };
        }
    }

    private static readonly string[] SpecialTypesIgnore = { "ByteArrayNetSerializable",
        "NodePoseSyncStateDeltaNetSerializable", "PlayerSpecificSettingsAtStartNetSerializable",
        "StandardScoreSyncStateDeltaNetSerializable", "BitMaskArray", "BitMaskSparse",
        "MultiplayerAvatarsData", "PlayersLobbyPermissionConfigurationNetSerializable",
        "PlayersMissingEntitlementsNetSerializable"
    };

    private DeserializeParser _deserializeParser = new();
    
    public void AnalyzeLine_SecondPass(LineAnalyzer line, Results results)
    {
        if (_typeName == null || SpecialTypesIgnore.Contains(_typeName))
            return;

        var result = _deserializeParser.FeedNextLine(line);
        if (result != null)
            _currentResult.DeserializeInstructions.Add(result);
    }
}