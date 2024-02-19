namespace BeatNet.CodeGen.Analysis.Structs;

public class TypedParam
{
    public string Name;
    public string TypeName;
    public bool IsConst = false; 
    public object? DefaultValue = null;
    
    public string NameForField => Name[..1].ToUpper() + Name[1..];
    public string NameForArg => Name[..1].ToLower() + Name[1..];
}