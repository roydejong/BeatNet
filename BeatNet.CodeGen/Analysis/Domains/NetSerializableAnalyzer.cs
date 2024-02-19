using System.Diagnostics;
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
        rawLine = rawLine.Replace("_color.", "");

        if (rawLine.Contains("reader."))
        {
            // Reader assign
            var eqIdx = rawLine.IndexOf('=');
            var refField = rawLine[..eqIdx].Trim();
            
            var dotIdx = rawLine.IndexOf("reader.", StringComparison.Ordinal);
            var callType = rawLine[(dotIdx + 7)..];

            var typecastEndIdx = rawLine.IndexOf(")reader.", StringComparison.Ordinal);
            string? typeCast = null;
            
            if (typecastEndIdx > 0)
            {
                var typecastStartIdx = rawLine.LastIndexOf('(', typecastEndIdx);
                typeCast = rawLine[(typecastStartIdx + 1)..typecastEndIdx];
                
                var typeCastDotIdx = typeCast.IndexOf('.');
                if (typeCastDotIdx > 0)
                    typeCast = typeCast[(typeCastDotIdx + 1)..];
                
                if (typeCast == "Type") // BG called this "Type" which, well, isn't ideal - so explicit fix
                    typeCast = "SliderType"; 
            }
            
            _currentResult.DeserializeInstructions.Add(new DeserializeInstruction()
            {
                CallType = callType,
                FieldName = refField.Trim('_'),
                TypeCast = typeCast
            });
        }
        else if (rawLine.Contains(".Deserialize"))
        {
            // Deserialize assign
            var hasEq = rawLine.Contains('=');

            if (hasEq)
            {
                // Assign the result of Object.Deserialize()
                var eqIdx = rawLine.IndexOf('=');
                var refField = rawLine[..eqIdx].Trim();
                var deserializeFn = rawLine[(eqIdx + 1)..].Trim();
            
                _currentResult.DeserializeInstructions.Add(new DeserializeInstruction()
                {
                    CallType = deserializeFn,
                    FieldName = refField.Trim('_')
                });
            }
            else
            {
                // Object.Deserialize() fills existing struct / no explicit assignment
                var dotIdx = rawLine.IndexOf(".Deserialize", StringComparison.Ordinal);
                var refField = rawLine[..dotIdx];
            
                _currentResult.DeserializeInstructions.Add(new DeserializeInstruction()
                {
                    CallType = "Deserialize();",
                    FieldName = refField.Trim('_')
                });
            }
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