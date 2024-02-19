using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;

namespace BeatNet.CodeGen.Generator.Util;

public static class FieldGenerator
{
    public static string GenerateFields(IResultWithFields item)
    {
        var fields = item.GetFields().ToList();
        
        var fieldBuffer = new StringBuilder();
        
        // Pass 1: Consts
        var anyConsts = false;
        
        foreach (var field in fields.Where(f => f.IsConst))
        {
            fieldBuffer.AppendLine($"\tpublic const {field.TypeName} {field.Name} = {field.DefaultValue};");
            anyConsts = true;
        }

        if (anyConsts)
            fieldBuffer.AppendLine();
        
        // Pass 2: Regular fields
        foreach (var field in fields.Where(f => !f.IsConst))
        {
            if (field.NameForField == item.GetSelfName())
                // Avoid "Member names cannot be the same as their enclosing type"
                field.Name += "Value";
            
            fieldBuffer.AppendLine($"\tpublic {field.TypeName} {field.NameForField} {{ get; set; }}");
        }
        
        return fieldBuffer.ToString();
    }
}