using System.Diagnostics;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Analysis.Structs;
using BeatNet.CodeGen.Analysis.Util;

namespace BeatNet.CodeGen.Analysis.Domains;

public class NetSerializableAnalyzer : ISubAnalyzer
{
    private string? _typeName = null;
    private readonly FieldParser _fieldParser = new();
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
                _currentResult.Fields.Add("r", new TypedParam() { Name = "r", TypeName = colorPartType });
                _currentResult.Fields.Add("g", new TypedParam() { Name = "g", TypeName = colorPartType });
                _currentResult.Fields.Add("b", new TypedParam() { Name = "b", TypeName = colorPartType });
                _currentResult.Fields.Add("a", new TypedParam() { Name = "a", TypeName = colorPartType });
            }

            return;
        }

        if (_typeName is null or "ColorSerializable" or "Color32Serializable" or "ColorNoAlphaSerializable")
            return;

        var field = _fieldParser.TryParse(line);
        if (field != null)
        {
            if (_typeName.Contains("BitMask") && field.Name is "bitCount" or "maxValue")
                return; // Ignore these fields, they're not part of the actual serialized data
            
            _currentResult.Fields[field.Name] = field;
        }
    }

    // These are tricky to parse / generate, these may end up using predefined code
    private static readonly string[] SpecialTypesIgnore = { "ByteArrayNetSerializable",
        "BitMaskArray", "BitMaskSparse", "NodePoseSyncStateDeltaNetSerializable", 
        "StandardScoreSyncStateDeltaNetSerializable", 
    };

    private DeserializeParser _deserializeParser = new();
    
    public void AnalyzeLine_SecondPass(LineAnalyzer line, Results results)
    {
        if (_typeName == null || SpecialTypesIgnore.Contains(_typeName))
            return;
        
        foreach (var instr in _deserializeParser.FeedNextLine(_currentResult, line))
            _currentResult.DeserializeInstructions.Add(instr);
    }
}