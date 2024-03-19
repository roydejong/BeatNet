using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;
using BeatNet.CodeGen.Utils;

namespace BeatNet.CodeGen.Generator.FixedImpl.NetSerializables;

/// <summary>
/// Adds network read/write implementation for SyncStateId.
/// </summary>
public class SyncStateIdFixedImpl : FixedImpl
{
    public override FixedImplMode Mode => FixedImplMode.Override;
    
    public override bool AppliesToType(string typeName)
    {
        return typeName == "SyncStateId";
    }

    public override void GenerateWriteTo(IResultWithFields item, StringBuilder buffer)
    {
        var fValue = GetValueField(item);
        var fFlag = GetFlagField(item);

        buffer.AppendLine($"\t\twriter.WriteByte((byte)({fValue.NameForField} | ({fFlag.NameForField} ? 128 : 0)));");
    }

    public override void GenerateReadFrom(IResultWithFields item, StringBuilder buffer)
    {
        var fValue = GetValueField(item);
        var fFlag = GetFlagField(item);
        
        buffer.AppendLine("\t\tvar @byte = reader.ReadByte();");
        buffer.AppendLine($"\t\t{fFlag.NameForField} = ((@byte & 128) > 0);");
        buffer.AppendLine($"\t\t{fValue.NameForField} = (byte)((int)@byte & -129);");
    }
    
    private static TypedParam GetValueField(IResultWithFields item) =>
        item.GetFields().First(field => !field.IsConst && field.TypeName == "byte");
    private static TypedParam GetFlagField(IResultWithFields item) =>
        item.GetFields().First(field => !field.IsConst && field.TypeName == "bool");
}