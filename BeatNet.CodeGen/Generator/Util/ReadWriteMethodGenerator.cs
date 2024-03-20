using System.Diagnostics;
using System.Text;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;
using BeatNet.CodeGen.Generator.FixedImpl;

namespace BeatNet.CodeGen.Generator.Util;

public static class ReadWriteMethodGenerator
{
    public static string GenerateMethods(IResultSerializable item, bool overrideKeyword = false)
    {
        var keywordPrefix = overrideKeyword ? "override " : "";
        
        var comboBuffer = new StringBuilder();
        
        var writeCodeBuffer = new StringBuilder();
        writeCodeBuffer.AppendLine($"\tpublic {keywordPrefix}void WriteTo(ref NetWriter writer)");
        writeCodeBuffer.AppendLine("\t{");

        var readCodeBuffer = new StringBuilder();
        readCodeBuffer.AppendLine($"\tpublic {keywordPrefix}void ReadFrom(ref NetReader reader)");
        readCodeBuffer.AppendLine("\t{");

        var fixedImpl = FixedImplManager.TryFindFixedImpl(item.GetSelfName());
        var hasFixedImpl = fixedImpl != null;
        
        if (fixedImpl is { Mode: FixedImplMode.Prefix or FixedImplMode.Override })
        {
            // Specific fixed implementation (prefix or override)
            writeCodeBuffer.AppendLine($"\t\t// {fixedImpl.GetType().Name}");
            fixedImpl.GenerateWriteTo(item, writeCodeBuffer);
            
            readCodeBuffer.AppendLine($"\t\t// {fixedImpl.GetType().Name}");
            fixedImpl.GenerateReadFrom(item, readCodeBuffer);
        }
        
        if (fixedImpl is not { Mode: FixedImplMode.Override })
        {
            // Generic read/write method generation (possibly prefixed by fixed implementation)
            GenerateFromInstructions(item, writeCodeBuffer, readCodeBuffer, hasFixedImpl);
        }
        
        if (fixedImpl is { Mode: FixedImplMode.Postfix })
        {
            // Specific fixed implementation (postfix)
            writeCodeBuffer.AppendLine($"\t\t// {fixedImpl.GetType().Name}");
            fixedImpl.GenerateWriteTo(item, writeCodeBuffer);
            
            readCodeBuffer.AppendLine($"\t\t// {fixedImpl.GetType().Name}");
            fixedImpl.GenerateReadFrom(item, readCodeBuffer);
        }
        
        writeCodeBuffer.AppendLine("\t}");
        readCodeBuffer.AppendLine("\t}");
        
        comboBuffer.Append(writeCodeBuffer);
        comboBuffer.AppendLine();
        comboBuffer.Append(readCodeBuffer);
        
        return comboBuffer.ToString();
    }

    private static void GenerateFromInstructions(IResultSerializable item, StringBuilder writeCodeBuffer, StringBuilder readCodeBuffer, bool hasFixedImpl)
    {
        var fields = item.GetFields().ToList();
        var instructions = item.GetInstructions().ToList();

        var anyFields = fields.Count > 0;
        var anyInstructions = instructions.Count > 0;
        
        var hasByteFlag = false;
        var byteFlagBitCount = 0;

        if (anyFields && anyInstructions)
        {
            if (hasFixedImpl)
            {
                writeCodeBuffer.AppendLine();
                readCodeBuffer.AppendLine();
            }

            for (var instructionIdx = 0; instructionIdx < instructions.Count(); instructionIdx++)
            {
                var instruction = instructions.ElementAt(instructionIdx);

                var linkedField = fields.FirstOrDefault(field =>
                    field.NameForField == instruction.FieldName || field.Name == instruction.FieldName);

                if (linkedField == null && instruction.TypeCast != null)
                {
                    // Try to match by field type instead, but only if there's one field of that type
                    var linkedFields = fields.Where(field => field.TypeName == instruction.TypeCast).ToList();
                    if (linkedFields.Count == 1)
                        linkedField = linkedFields.First();
                }
                
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

                    if (!hasFixedImpl)
                    {
                        // No fixed implementation, and a field that cannot be auto mapped
                        writeCodeBuffer.AppendLine($"\t\t// TODO Bad Field Ref: {instruction.FieldName} / {instruction.CallType} / {instruction.TypeCast}");
                        readCodeBuffer.AppendLine($"\t\t// TODO Bad Field Ref: {instruction.FieldName} / {instruction.CallType} / {instruction.TypeCast}");

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"WARNING: Incomplete type: `{item.GetSelfName()}`");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine($"DEBUG: Possible incomplete type: `{item.GetSelfName()}` -- Field: {instruction.FieldName} -- CallType: {instruction.CallType}");
                        Console.ResetColor();
                    }

                    continue;
                }

                var rwMethod = $"Serializable<{linkedField.TypeName}>";
                var shouldTypeCast = instruction.TypeCast != null;
                var readCastPrefix = shouldTypeCast ? $"({instruction.TypeCast})" : "";
                var writeCastPrefix = "";
                switch (instruction.CallType.Trim(';'))
                {
                    case "GetVarULong()":
                        rwMethod = "VarULong";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(ulong)";
                        break;
                    case "GetVarUInt()":
                        rwMethod = "VarUInt";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(uint)";
                        break;
                    case "GetVarLong()":
                        rwMethod = "VarLong";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(long)";
                        break;
                    case "GetVarInt()":
                        rwMethod = "VarInt";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(int)";
                        break;
                    case "GetString()":
                        rwMethod = "String";
                        break;
                    case "GetBool()":
                        rwMethod = "Bool";
                        break;
                    case "GetFloat()":
                        rwMethod = "Float";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(float)";
                        break;
                    case "GetByte()":
                        rwMethod = "Byte";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(byte)";
                        break;
                    case "GetULong()":
                        rwMethod = "ULong";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(ulong)";
                        break;
                    case "GetUInt()":
                        rwMethod = "UInt";
                        if (shouldTypeCast)
                            writeCastPrefix = $"(uint)";
                        break;
                    case "1f":
                        rwMethod = null;
                        // Do not write 1f, but assign it on read
                        readCodeBuffer.AppendLine($"\t\t{linkedField.NameForField} = 1f;");
                        break;
                    case "Deserialize()":
                        // "X = obj.Deserialize()" or simply "X.Deserialize()" -- INetSerializable
                        if (instruction.TypeCast?.Contains("List<") ?? false)
                        {
                            var listInnerType = instruction.TypeCast!.Substring(5, instruction.TypeCast.Length - 6);
                            rwMethod = $"SerializableList<{instruction.TypeCast}, {listInnerType}>";
                        }
                        writeCastPrefix = null;
                        readCastPrefix = null;
                        break;
                    default:
                        if (instruction.CallType.Contains("@byte"))
                        {
                            var bitShift = 1 << byteFlagBitCount;
                            rwMethod = null;
                            writeCodeBuffer.AppendLine(
                                $"\t\tflags |= (byte)({linkedField.NameForField} ? {bitShift} : 0);");
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

                    writeCodeBuffer.AppendLine(
                        $"\t\twriter.Write{rwMethod}({writeCastPrefix}{linkedField.NameForField});");
                    readCodeBuffer.AppendLine(
                        $"\t\t{linkedField.NameForField} = {readCastPrefix}reader.Read{rwMethod}();");
                }
            }
        }
        else
        {
            // Regardless of FixedImpl, we were expected to do something but have no instructions to do so
            writeCodeBuffer.AppendLine("\t\tthrow new NotImplementedException(); // TODO");
            readCodeBuffer.AppendLine("\t\tthrow new NotImplementedException(); // TODO");
                    
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARNING: Incomplete type: `{item.GetSelfName()}`");
            Console.ResetColor();
        }

        if (hasByteFlag)
        {
            // Ending byte flag group (packed bools) - last field
            writeCodeBuffer.AppendLine($"\t\twriter.WriteByte(flags);");
        }
    }
}