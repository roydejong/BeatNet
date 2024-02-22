using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;
using BeatNet.CodeGen.Analysis.Structs;
using BeatNet.CodeGen.Utils;

namespace BeatNet.CodeGen.Generator.FixedImpl.NetSerializables;

/// <summary>
/// Adds network read/write implementation for GameplayModifiers.
/// </summary>
public class GameplayModifiersFixedImpl : FixedImpl
{
    public override FixedImplMode Mode => FixedImplMode.Override;
    
    public override bool AppliesToType(string typeName)
    {
        return typeName == "GameplayModifiers";
    }

    private const int BitsForIntEnums = 4;
    private const int BitMaskForIntEnums = (1 << BitsForIntEnums) - 1;
    
    // BG is all over the place with the order, so we'll hardcode the known ones and assume any new ones come in order after those
    private static readonly List<string> knownFields = new() { "energyType", "instaFail", "failOnSaberClash", "enabledObstacleType", 
        "noBombs", "fastNotes", "strictAngles", "disappearingArrows", "ghostNotes", "songSpeed", "noArrows",
        "noFailOn0Energy", "proMode", "zenMode", "smallCubes" };

    private List<TypedParam> GetSortedFields(IResultWithFields item)
    {
        var sortedFields = new List<TypedParam>();
        var fields = item.GetFields().ToList();
        
        foreach (var fieldName in knownFields)
        {
            var field = fields.FirstOrDefault(f => f.NameForField == fieldName || f.NameForArg == fieldName);
            if (field == null)
                throw new Exception("Well this shouldn't happen");
            sortedFields.Add(field);
        }

        foreach (var field in fields)
        {
            if (!sortedFields.Contains(field))
                sortedFields.Add(field);
        }

        return sortedFields;
    }
    
    // BG has presumably changed the structure over time, some bits are seemingly left unused
    private static readonly Dictionary<string, int> bitSkips = new()
    {
        { "EnergyType", 2 }, // after EnergyType enum, (0 + 4) skips to 6
        { "EnabledObstacleType", 1 }, // after EnabledObstacleType enum, (8 + 4) skips to 13
    };

    public override void GenerateWriteTo(IResultWithFields item, StringBuilder buffer)
    {
        buffer.AppendLine($"\t\tvar packed = 0;");
        
        var bitIndex = 0;
        var fields = GetSortedFields(item);

        foreach (var field in fields)
        {
            switch (field.TypeName)
            {
                case "bool":
                    // Packed bool
                    buffer.AppendLine($"\t\tpacked |= {field.NameForField} ? 1 << {bitIndex} : 0;");
                    bitIndex++;
                    break;
                default:
                    // Packed enum (4-bit int)
                    buffer.AppendLine($"\t\tpacked |= (int)({field.NameForField} & ({field.TypeName}){BitMaskForIntEnums}) << {bitIndex};");
                    bitIndex += BitsForIntEnums;
                    break;
            }

            if (bitSkips.TryGetValue(field.NameForField, out var bitSkip))
                bitIndex += bitSkip;
        }
        
        buffer.AppendLine($"\t\twriter.WriteInt(packed);");
    }

    public override void GenerateReadFrom(IResultWithFields item, StringBuilder buffer)
    {
        buffer.AppendLine($"\t\tvar packed = reader.ReadInt();");
        
        var bitIndex = 0;
        var fields = GetSortedFields(item);

        foreach (var field in fields)
        {
            switch (field.TypeName)
            {
                case "bool":
                    // Packed bool
                    buffer.AppendLine($"\t\t{field.NameForField} = (packed & (1 << {bitIndex})) != 0;");
                    bitIndex++;
                    break;
                default:
                    // Packed enum (4-bit int)
                    buffer.AppendLine($"\t\t{field.NameForField} = ({field.TypeName})((packed >> {bitIndex}) & {BitMaskForIntEnums});");
                    bitIndex += BitsForIntEnums;
                    break;
            }

            if (bitSkips.TryGetValue(field.NameForField, out var bitSkip))
                bitIndex += bitSkip;
        }
    }
}