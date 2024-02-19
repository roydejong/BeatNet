using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.Util;

public static class FieldParser
{
    public static TypedParam? TryParse(LineAnalyzer line)
    {
        if (!line.IsField)
            return null;
        
        var name = line.DeclaredName!;
        var type = line.DeclaredType!;
        
        name = name.Trim('_');

        if (name.Contains("__BackingField"))
            // Ignore compiler generated backing fields
            return null;

        if (type.Contains("IReadOnly"))
            // Ignore read only lists
            return null;

        if (type.Contains("PacketPool") || name == "pool")
            // Ignore packet pools
            return null;

        if (name == "sliderType")
            type = "SliderType"; // BG called this "Type" which, well, isn't ideal - so explicit fix

        return new TypedParam()
        {
            TypeName = type,
            Name = name,
            IsConst = line.Const,
            DefaultValue = line.DefaultValue
        };
    }
}