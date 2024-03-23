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

            var inArgsList = !field.IsFixedInit;
            
            if (paramNo > 0 && inArgsList)
                constructorBuffer.Append(", ");

            var hasComplexDefaultValue = (field.DefaultValue?.ToString() ?? "").StartsWith("new ");
            if (inArgsList)
            {
                if (field.DefaultNull)
                {
                    constructorBuffer.Append($"{field.TypeName}? {field.NameForArg} = null");
                }
                else if (field.DefaultValue != null)
                {
                    if (hasComplexDefaultValue)
                    {
                        constructorBuffer.Append($"{field.TypeName}? {field.NameForArg} = null");
                        hasComplexDefaultValue = true;
                    }
                    else
                    {
                        constructorBuffer.Append($"{field.TypeName} {field.NameForArg} = {field.DefaultValue}");
                    }
                }
                else
                {
                    constructorBuffer.Append($"{field.TypeName} {field.NameForArg}");
                }
            }

            if (field.IsFixedInit)
                constructorBodyBuffer.AppendLine($"\t\t{field.NameForField} = {field.DefaultValue};");
            else if (hasComplexDefaultValue)
                constructorBodyBuffer.AppendLine($"\t\t{field.NameForField} = {field.NameForArg} ?? {field.DefaultValue};");
            else
                constructorBodyBuffer.AppendLine($"\t\t{field.NameForField} = {field.NameForArg};");
            
            if (inArgsList)
                paramNo++;
        }
        
        constructorBuffer.AppendLine(")");
        constructorBuffer.AppendLine($"\t{{");
        constructorBuffer.Append(constructorBodyBuffer);
        constructorBuffer.AppendLine($"\t}}");
        
        return constructorBuffer.ToString();
    }
}