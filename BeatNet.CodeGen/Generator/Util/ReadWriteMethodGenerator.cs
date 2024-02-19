using System.Diagnostics;
using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;

namespace BeatNet.CodeGen.Generator.Util;

public static class ReadWriteMethodGenerator
{
    public static string GenerateMethods(IResultWithFieldsAndInstructions item)
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
        
        var hasByteFlag = false;
        var byteFlagBitCount = 0;
        
        if (anyInstructions)
        {
            for (var instructionIdx = 0; instructionIdx < instructions.Count(); instructionIdx++)
            {
                var instruction = instructions.ElementAt(instructionIdx);
                
                var linkedField = fields.FirstOrDefault(field => 
                    field.NameForField == instruction.FieldName || field.Name == instruction.FieldName);

                if (linkedField == null)
                { 
                    if (instruction.FieldName == "byte @byte")
                    {
                        // Starting byte flag group (packed bools)
                        hasByteFlag = true;
                        byteFlagBitCount = 0;
                        
                        writeCodeBuffer.AppendLine("\t\tbyte flags = 0;");
                        readCodeBuffer.AppendLine("\t\tvar flags = reader.ReadByte();");
                        continue;
                    }
                    
                    Debugger.Break();
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
                        readCodeBuffer.AppendLine($"\t\t{linkedField.NameForField} = 1f;");
                        break;
                    case "MultiplayerAvatarsData.Deserialize(reader);":
                    case "PlayerStateHash.Deserialize(reader);":
                    case "SyncStateId.Deserialize(reader);":
                    case "Deserialize();":
                        // INetSerializable
                        break;
                    default:
                        if (instruction.CallType.Contains("@byte"))
                        {
                            var bitShift = 1 << byteFlagBitCount;
                            rwMethod = null;
                            writeCodeBuffer.AppendLine($"\t\tflags |= (byte)({linkedField.NameForField} ? {bitShift} : 0);");
                            readCodeBuffer.AppendLine($"\t\t{linkedField.NameForField} = (flags & {bitShift}) != 0;");
                            byteFlagBitCount++;
                        }
                        else
                        {
                            Debugger.Break();
                        }
                        break;
                }

                if (rwMethod != null)
                {
                    if (hasByteFlag)
                    {
                        // Ending byte flag group (packed bools)
                        writeCodeBuffer.AppendLine($"\t\twriter.WriteByte(flags);");
                        hasByteFlag = false;
                        byteFlagBitCount = 0;
                    }
                    
                    writeCodeBuffer.AppendLine($"\t\twriter.Write{rwMethod}({writeCastPrefix}{linkedField.NameForField});");
                    readCodeBuffer.AppendLine($"\t\t{linkedField.NameForField} = {readCastPrefix}reader.Read{rwMethod}();");
                }
            }
        }
        else
        {
            writeCodeBuffer.AppendLine("\t\tthrow new NotImplementedException(); // TODO");
            readCodeBuffer.AppendLine("\t\tthrow new NotImplementedException(); // TODO");
        }
        
        if (hasByteFlag)
        {
            // Ending byte flag group (packed bools) - last field
            writeCodeBuffer.AppendLine($"\t\twriter.WriteByte(flags);");
        }
        
        writeCodeBuffer.AppendLine("\t}");
        comboBuffer.Append(writeCodeBuffer);
        comboBuffer.AppendLine();
        
        readCodeBuffer.AppendLine("\t}");
        comboBuffer.Append(readCodeBuffer);

        return comboBuffer.ToString();
    }
}