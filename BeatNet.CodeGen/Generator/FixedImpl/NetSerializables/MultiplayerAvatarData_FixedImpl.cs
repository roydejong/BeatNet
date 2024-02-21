using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Generator.FixedImpl.NetSerializables;

/// <summary>
/// Adds network read/write implementation for MultiplayerAvatarData.
/// </summary>
public class MultiplayerAvatarDataFixedImpl : FixedImpl
{
    public override FixedImplMode Mode => FixedImplMode.Override;
    
    public override bool AppliesToType(string typeName)
    {
        return typeName == "MultiplayerAvatarData";
    }

    public override void GenerateWriteTo(IResultWithFields item, StringBuilder buffer)
    {
        var fHash = GetHashField(item);
        var fBytes = GetBytesField(item);
        
        buffer.AppendLine($"\t\twriter.WriteUInt({fHash.NameForField});");
        buffer.AppendLine($"\t\twriter.WriteByteArray({fBytes.NameForField});");
    }

    public override void GenerateReadFrom(IResultWithFields item, StringBuilder buffer)
    {
        var fHash = GetHashField(item);
        var fBytes = GetBytesField(item);
        
        buffer.AppendLine($"\t\t{fHash.NameForField} = reader.ReadUInt();");
        buffer.AppendLine($"\t\t{fBytes.NameForField} = reader.ReadByteArray();");
    }

    private static TypedParam GetHashField(IResultWithFields item) =>
        item.GetFields().First(field => field.TypeName == "uint");
    private static TypedParam GetBytesField(IResultWithFields item) =>
        item.GetFields().First(field => field.TypeName == "byte[]");
}