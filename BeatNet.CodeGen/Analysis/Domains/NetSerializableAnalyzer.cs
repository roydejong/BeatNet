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
        }

        if (_typeName == null)
            return;

        if (line.IsField)
        {
            var name = line.DeclaredName!;
            name = name.Trim('_');

            if (name.Contains("__BackingField"))
                return;
            
            _currentResult.Fields.Add(new TypedParam()
            {
                TypeName = line.DeclaredType!,
                ParamName = name
            });
        }
    }

    public void AnalyzeLine_SecondPass(LineAnalyzer line, Results results)
    {
        
    }
}