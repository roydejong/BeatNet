namespace BeatNet.CodeGen.Analysis.Structs;

public class MethodParam
{
    public string TypeName;
    public string ParamName;
    
    public string ParamNameForField => ParamName[..1].ToUpper() + ParamName[1..];
    public string ParamNameForArg => ParamName[..1].ToLower() + ParamName[1..];
}