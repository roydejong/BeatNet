using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;
using BeatNet.CodeGen.Utils;

namespace BeatNet.CodeGen.Generator.FixedImpl.NetSerializables;

/// <summary>
/// Adds network read/write implementation for MultiplayerLevelCompletionResults.
/// </summary>
public class MultiplayerLevelCompletionResultsFixedImpl : FixedImpl
{
    public override FixedImplMode Mode => FixedImplMode.Override;
    
    public override bool AppliesToType(string typeName)
    {
        return typeName == "MultiplayerLevelCompletionResults";
    }

    public override void GenerateWriteTo(IResultWithFields item, StringBuilder buffer)
    {
        var fEndState = GetEndStateField(item);
        var fEndReason = GetEndReasonField(item);
        var fEndResults = GetEndResultsField(item);
        
        buffer.AppendLine($"\t\twriter.WriteVarInt((int){fEndState.NameForField});");
        buffer.AppendLine($"\t\twriter.WriteVarInt((int){fEndReason.NameForField});");
        buffer.AppendLine($"\t\tvar hasAnyResults = ({fEndState.NameForField} is {fEndState.TypeName}.SongFinished or {fEndState.TypeName}.NotFinished);");
        buffer.AppendLine($"\t\tif (hasAnyResults)");
        buffer.AppendLine($"\t\t\twriter.WriteSerializable({fEndResults.NameForField});");
    }

    public override void GenerateReadFrom(IResultWithFields item, StringBuilder buffer)
    {
        var fEndState = GetEndStateField(item);
        var fEndReason = GetEndReasonField(item);
        var fEndResults = GetEndResultsField(item);
        
        buffer.AppendLine($"\t\t{fEndState.NameForField} = ({fEndState.TypeName})reader.ReadVarInt();");
        buffer.AppendLine($"\t\t{fEndReason.NameForField} = ({fEndReason.TypeName})reader.ReadVarInt();");
        buffer.AppendLine($"\t\tvar hasAnyResults = ({fEndState.NameForField} is {fEndState.TypeName}.SongFinished or {fEndState.TypeName}.NotFinished);");
        buffer.AppendLine($"\t\tif (hasAnyResults)");
        buffer.AppendLine($"\t\t\t{fEndResults.NameForField} = reader.ReadSerializable<{fEndResults.TypeName}>();");
    }
    
    private static TypedParam GetEndStateField(IResultWithFields item) =>
        item.GetFields().First(field => !field.IsConst && field.TypeName.Contains("MultiplayerPlayerLevelEndState"));
    private static TypedParam GetEndReasonField(IResultWithFields item) =>
        item.GetFields().First(field => !field.IsConst && field.TypeName.Contains("MultiplayerPlayerLevelEndReason"));
    private static TypedParam GetEndResultsField(IResultWithFields item) =>
        item.GetFields().First(field => !field.IsConst && field.TypeName.Contains("LevelCompletionResults"));
}