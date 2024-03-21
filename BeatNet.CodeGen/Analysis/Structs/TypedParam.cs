namespace BeatNet.CodeGen.Analysis.Structs;

public class TypedParam
{
    public string Name;
    public string TypeName;
    public bool IsConst = false;
    public object? DefaultValue = null;
    public bool DefaultNull = false;

    public string NameForField => Name[..1].ToUpper() + Name[1..];
    public string NameForArg => Name[..1].ToLower() + Name[1..];

    public string? TryGetGenericFromType()
    {
        var start = TypeName.IndexOf('<');
        var end = TypeName.LastIndexOf('>');
        
        if (start == -1 || end == -1)
            return null;
        
        return TypeName[(start + 1)..end];
    }
}