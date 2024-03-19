using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;
using BeatNet.CodeGen.Utils;

namespace BeatNet.CodeGen.Generator.FixedImpl.NetSerializables;

/// <summary>
/// Adds network read/write implementation for SyncStateDelta.
/// </summary>
public class SyncStateDeltaFixedImpl : FixedImpl
{
    public override FixedImplMode Mode => FixedImplMode.Override;
    
    public override bool AppliesToType(string typeName)
    {
        return typeName.EndsWith("SyncStateDeltaNetSerializable");
    }

    public override void GenerateWriteTo(IResultWithFields item, StringBuilder buffer)
    {
        var fId = GetIdField(item);
        var fTime = GetTimeField(item);
        var fDelta = GetDeltaField(item);
        
        buffer.AppendLine($"\t\twriter.WriteSerializable({fId.NameForField});");
        buffer.AppendLine($"\t\twriter.WriteVarInt({fTime.NameForField});");
        buffer.AppendLine($"\t\tif (!{fId.NameForField}.Flag)");
        buffer.AppendLine($"\t\t\twriter.WriteSerializable({fDelta.NameForField});");
    }

    public override void GenerateReadFrom(IResultWithFields item, StringBuilder buffer)
    {
        var fId = GetIdField(item);
        var fTime = GetTimeField(item);
        var fDelta = GetDeltaField(item);
        
        buffer.AppendLine($"\t\t{fId.NameForField} = reader.ReadSerializable<SyncStateId>();");
        buffer.AppendLine($"\t\t{fTime.NameForField} = reader.ReadVarInt();");
        buffer.AppendLine($"\t\tif (!{fId.NameForField}.Flag)");
        buffer.AppendLine($"\t\t\t{fDelta.NameForField} = reader.ReadSerializable<{fDelta.TypeName}>();");
    }
    
    private static TypedParam GetIdField(IResultWithFields item) =>
        item.GetFields().First(field => !field.IsConst && field.TypeName == "SyncStateId");
    private static TypedParam GetTimeField(IResultWithFields item) =>
        item.GetFields().First(field => !field.IsConst && field.TypeName == "int");
    private static TypedParam GetDeltaField(IResultWithFields item) =>
        item.GetFields().First(field => !field.IsConst && field.NameForField.EndsWith("Delta"));
}