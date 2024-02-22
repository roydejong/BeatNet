using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Generator.FixedImpl.NetSerializables;

/// <summary>
/// Adds network read/write implementation for some generic types that are just a list of a serializable type.
/// </summary>
public class GenericListTypesFixedImpl : FixedImpl
{
    public override FixedImplMode Mode => FixedImplMode.Override;
    
    public override bool AppliesToType(string typeName)
    {
        return typeName is "PlayersLobbyPermissionConfigurationNetSerializable"
            or "PlayersMissingEntitlementsNetSerializable";
    }

    public override void GenerateWriteTo(IResultWithFields item, StringBuilder buffer)
    {
        var listField = GetListField(item);
        var genericType = listField.TryGetGenericFromType();

        if (genericType == null)
            throw new Exception("Expection failed: could not get generic type from list field");

        if (genericType == "string")
        {
            buffer.AppendLine($"\t\twriter.WriteStringList({listField.NameForField});");
        }
        else
        {
            buffer.AppendLine($"\t\twriter.WriteSerializableList<List<{genericType}>, {genericType}>({listField.NameForField});");   
        }
    }

    public override void GenerateReadFrom(IResultWithFields item, StringBuilder buffer)
    {
        var listField = GetListField(item);
        var genericType = listField.TryGetGenericFromType();

        if (genericType == null)
            throw new Exception("Expection failed: could not get generic type from list field");
        
        if (genericType == "string")
        {
            buffer.AppendLine($"\t\t{listField.NameForField} = reader.ReadStringList();");
        }
        else
        {
            buffer.AppendLine($"\t\t{listField.NameForField} = reader.ReadSerializableList<List<{genericType}>, {genericType}>();");   
        }
    }

    private static TypedParam GetListField(IResultWithFields item) =>
        item.GetFields().First(field => field.TypeName.StartsWith("List<"));
}