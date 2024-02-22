using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.Util;

public class FieldParser
{
    private bool _evaluatingProperty = false;
    private TypedParam? _curProperty = null;
    private int _scopeLevel = 0;
    private int? _propertyScopeLevel = null;
    private bool _foundGetter = false;
    private bool _foundSetter = false;
    
    public TypedParam? TryParse(LineAnalyzer line)
    {
        if (line.IsOpenBracket)
        {
            _scopeLevel++;
            return null;
        }
        else if (line.IsCloseBracket)
        {
            _scopeLevel--;
            if (_evaluatingProperty && _scopeLevel <= _propertyScopeLevel!.Value && _foundGetter && _foundSetter)
            {
                _evaluatingProperty = false;
                return _curProperty;
            }
            return null;
        }
        else if (line.RawLine.EndsWith("get"))
        {
            if (_evaluatingProperty) 
                _foundGetter = true;
            return null;
        }
        else if (line.RawLine.EndsWith("set"))
        {
            if (_evaluatingProperty) 
                _foundSetter = true;
            return null;
        }

        if (line is { IsField: false, IsProperty: false })
            return null;

        if (line is { Static: true, ReadOnly: true })
            return null;
        
        if (_evaluatingProperty) 
            _evaluatingProperty = false;
        
        var name = line.DeclaredName!;
        var type = line.DeclaredType!;
        
        name = name.Trim('_');

        if (name.Contains("__BackingField"))
            // Ignore compiler generated backing fields
            return null;

        if (type.Contains("IReadOnly"))
            // Convert read only list types (IReadOnlyList -> List)
            type = type.Replace("IReadOnly", "");

        if (type.Contains("PacketPool") || name == "pool")
            // Ignore packet pools
            return null;

        if (type == "List<IConnectedPlayer>")
            // PlayerSpecificSettingsAtStartNetSerializable specific (works as general rule because IConnectedPlayer cannot serialize over network)
            return null;

        if (name == "sliderType")
            type = "SliderType"; // BG called this "Type" which, well, isn't ideal - so explicit fix
        
        var result = new TypedParam()
        {
            TypeName = type,
            Name = name,
            IsConst = line.Const,
            DefaultValue = line.DefaultValue
        };

        if (line.IsProperty)
        {
            _evaluatingProperty = true;
            _curProperty = result;
            _propertyScopeLevel = _scopeLevel;
            _foundGetter = false;
            _foundSetter = false;
            return null;
        }

        return result;
    }
}