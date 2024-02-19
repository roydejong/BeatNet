using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;

namespace BeatNet.CodeGen.Generator.Util;

public static class ConstructorGenerator
{
    public static string GenerateConstructor(IResultWithFields item)
    {
        var fields = item.GetFields().ToList();
        
        var constructorBuffer = new StringBuilder();
        var constructorBodyBuffer = new StringBuilder();
        
        constructorBuffer.Append($"\tpublic {item.GetSelfName()}(");

        var paramNo = 0;
        foreach (var field in fields)
        {
            if (field.IsConst)
                continue;
            
            if (paramNo > 0)
                constructorBuffer.Append(", ");
            
            constructorBuffer.Append($"{field.TypeName} {field.NameForArg}");
            constructorBodyBuffer.AppendLine($"\t\t{field.NameForField} = {field.NameForArg};");
            
            paramNo++;
        }
        
        constructorBuffer.AppendLine(")");
        constructorBuffer.AppendLine($"\t{{");
        constructorBuffer.Append(constructorBodyBuffer);
        constructorBuffer.AppendLine($"\t}}");
        
        return constructorBuffer.ToString();
    }
}