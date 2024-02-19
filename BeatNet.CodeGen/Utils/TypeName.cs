namespace BeatNet.CodeGen.Utils;

public static class TypeName
{
    public static bool IsPrimitive(string typeName)
    {
        return typeName switch
        {
            "bool" => true,
            "byte" => true,
            "sbyte" => true,
            "char" => true,
            "decimal" => true,
            "double" => true,
            "float" => true,
            "int" => true,
            "uint" => true,
            "long" => true,
            "ulong" => true,
            "short" => true,
            "ushort" => true,
            "string" => true,
            _ => false
        };
    }
}