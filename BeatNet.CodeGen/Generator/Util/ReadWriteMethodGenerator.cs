using System.Diagnostics;
using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;

namespace BeatNet.CodeGen.Generator.Util;

public static class ReadWriteMethodGenerator
{
    public static string GenerateMethods(ISerializeDeserialize item)
    {
        var fields = item.GetFields().ToList();
        var instructions = item.GetInstructions().ToList();

        var anyFields = fields.Count > 0;
        var anyInstructions = instructions.Count > 0;

        var comboBuffer = new StringBuilder();
        
        var writeCodeBuffer = new StringBuilder();
        writeCodeBuffer.AppendLine("\tpublic void WriteTo(ref NetWriter writer)");
        writeCodeBuffer.AppendLine("\t{");
        
        var readCodeBuffer = new StringBuilder();
        readCodeBuffer.AppendLine("\tpublic void ReadFrom(ref NetReader reader)");
        readCodeBuffer.AppendLine("\t{");
        
        if (anyInstructions)
        {
            for (var instructionIdx = 0; instructionIdx < instructions.Count(); instructionIdx++)
            {
                var instruction = instructions.ElementAt(instructionIdx);
                var linkedField = fields.FirstOrDefault(field => 
                    field.ParamNameForField == instruction.FieldName || field.ParamName == instruction.FieldName);

                if (linkedField == null)
                { 
                    // Debugger.Break();
                    writeCodeBuffer.AppendLine($"\t\t// TODO Bad Field Ref: {instruction.FieldName}");
                    readCodeBuffer.AppendLine($"\t\t// TODO Bad Field Ref: {instruction.FieldName}");
                    continue;
                }
                
                var rwMethod = $"Serializable<{linkedField.TypeName}>";
                var shouldTypeCast = instruction.TypeCast != null;
                var readCastPrefix = shouldTypeCast ? $"({instruction.TypeCast})" : "";
                var writeCastPrefix = "";
                switch (instruction.CallType)
                {
                    case "GetVarULong();":
                        rwMethod = "VarULong";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(ulong)";
                        break;
                    case "GetVarUInt();":
                        rwMethod = "VarUInt";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(uint)";
                        break;
                    case "GetVarLong();":
                        rwMethod = "VarLong";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(long)";
                        break;
                    case "GetVarInt();":
                        rwMethod = "VarInt";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(int)";
                        break;
                    case "GetString();":
                        rwMethod = "String";
                        break;
                    case "GetBool();":
                        rwMethod = "Bool";
                        break;
                    case "GetFloat();":
                        rwMethod = "Float";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(float)";
                        break;
                    case "GetByte();":
                        rwMethod = "Byte";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(byte)";
                        break;
                    case "1f":
                        rwMethod = null;
                        writeCodeBuffer.AppendLine($"\t\twriter.WriteFloat(1f);");
                        readCodeBuffer.AppendLine($"\t\t{linkedField.ParamNameForField} = 1f;");
                        break;
                    case "SyncStateId.Deserialize(reader);":
                    case "Deserialize();":
                        // INetSerializable
                        break;
                    default:
                        Debugger.Break();
                        break;
                }

                if (rwMethod != null)
                {
                    writeCodeBuffer.AppendLine($"\t\twriter.Write{rwMethod}({writeCastPrefix}{linkedField.ParamNameForField});");
                    readCodeBuffer.AppendLine($"\t\t{linkedField.ParamNameForField} = {readCastPrefix}reader.Read{rwMethod}();");
                }
            }
        }
        else
        {
            writeCodeBuffer.AppendLine("\t\tthrow new NotImplementedException(); // TODO");
            readCodeBuffer.AppendLine("\t\tthrow new NotImplementedException(); // TODO");
        }
        
        writeCodeBuffer.AppendLine("\t}");
        comboBuffer.Append(writeCodeBuffer);
        comboBuffer.AppendLine();
        
        readCodeBuffer.AppendLine("\t}");
        comboBuffer.Append(readCodeBuffer);

        return comboBuffer.ToString();
    }
}