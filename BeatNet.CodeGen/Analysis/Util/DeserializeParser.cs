using System.Diagnostics;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Analysis.ResultData.Common;

namespace BeatNet.CodeGen.Analysis.Util;

public class DeserializeParser
{
    private bool _inDeserialize = false;
    private bool _inStaticDeserialize = false;
    private int _deserializeDepth = 0;
    
    public IEnumerable<DeserializeInstruction> FeedNextLine(IResultWithFields item, LineAnalyzer line)
    {
        if (!_inDeserialize)
        {
            _deserializeDepth = 0;

            if (line.IsMethod && line.DeclaredName!.Contains("Deserialize"))
            {
                _inDeserialize = true;
                _inStaticDeserialize = line.Static;
            }

            yield break;
        }

        if (line.IsOpenBracket)
        {
            _deserializeDepth++;
            yield break;
        } 
        else if (line.IsCloseBracket)
        {
            _deserializeDepth--;
            yield break;
        }

        if (_deserializeDepth == 0)
        {
            _inDeserialize = false;
            yield break;
        }

        var rawLine = line.RawLine;
        rawLine = rawLine.Replace("this.", "");
        rawLine = rawLine.Replace("_color.", "");

        if (rawLine.Contains("return") && _inStaticDeserialize)
        {
            // Return statements are used by immutable structs
            // e.g. "return new BitMask128(reader.GetULong(), reader.GetULong());"
            
            var newIdx = rawLine.IndexOf("new", StringComparison.Ordinal);
            var fieldStartIdx = rawLine.IndexOf("(", StringComparison.Ordinal);
            
            var newTypeName = rawLine[(newIdx + 3)..fieldStartIdx].Trim();
            var fieldList = rawLine[(fieldStartIdx + 1)..^2].Split(',');

            var fieldIndex = 0;
            foreach (var field in fieldList)
            {
                var linkedField = item.GetFields().Where(f => !f.IsConst).ElementAt(fieldIndex);
                
                if (field.Contains("reader."))
                {
                    var dotIdx = field.IndexOf("reader.", StringComparison.Ordinal);
                    var callType = field[(dotIdx + 7)..];
                
                    yield return new DeserializeInstruction()
                    {
                        CallType = callType,
                        FieldName = linkedField.Name,
                        TypeCast = linkedField.TypeName
                    };
                }
                else if (field.Contains(".Deserialize"))
                {
                    var dotIdx = field.IndexOf(".Deserialize", StringComparison.Ordinal);
                    var refType = field[..dotIdx];
                    
                    string? typeCast = null;
                    if (refType != linkedField.TypeName)
                        typeCast = refType;
                    
                    yield return new DeserializeInstruction()
                    {
                        CallType = "Deserialize();",
                        FieldName = linkedField.Name,
                        TypeCast = typeCast
                    };
                }
                else
                {
                    // Parser expectation failed: expected reader or deserialize call in static return
                }
                
                fieldIndex++;
            }
        }
        else if (rawLine.Contains("reader.") && !rawLine.Contains(".Add"))
        {
            // Reader assign
            var eqIdx = rawLine.IndexOf('=');
            var refField = rawLine[..eqIdx].Trim();
            
            var dotIdx = rawLine.IndexOf("reader.", StringComparison.Ordinal);
            var callType = rawLine[(dotIdx + 7)..];

            var typecastEndIdx = rawLine.IndexOf(")reader.", StringComparison.Ordinal);
            string? typeCast = null;
            
            if (typecastEndIdx > 0)
            {
                var typecastStartIdx = rawLine.LastIndexOf('(', typecastEndIdx);
                typeCast = rawLine[(typecastStartIdx + 1)..typecastEndIdx];
                
                var typeCastDotIdx = typeCast.IndexOf('.');
                if (typeCastDotIdx > 0)
                    typeCast = typeCast[(typeCastDotIdx + 1)..];
                
                if (typeCast == "Type") // BG called this "Type" which, well, isn't ideal - so explicit fix
                    typeCast = "SliderType"; 
            }
            
            yield return new DeserializeInstruction()
            {
                CallType = callType,
                FieldName = refField.Trim('_'),
                TypeCast = typeCast
            };
            yield break;
        }
        else if (rawLine.Contains(".Deserialize"))
        {
            // Deserialize assign
            var hasEq = rawLine.Contains('=');
            string refField;
            string? refType = null;
            
            if (hasEq)
            {
                // Assign the result of Object.Deserialize()
                var eqIdx = rawLine.IndexOf('=');
                refField = rawLine[..eqIdx].Trim();

                var spaceIdx = rawLine.IndexOf(' ', 0, eqIdx - 1);
                if (spaceIdx > 0)
                {
                    refType = rawLine[..spaceIdx].Trim();;
                    refField = rawLine[(spaceIdx + 1)..eqIdx].Trim();
                }
            }
            else
            {
                // Object.Deserialize() fills existing struct / no explicit assignment
                var dotIdx = rawLine.IndexOf(".Deserialize", StringComparison.Ordinal);
                refField = rawLine[..dotIdx];
            }
            
            yield return new DeserializeInstruction()
            {
                CallType = "Deserialize();",
                FieldName = refField.Trim('_'),
                TypeCast = refType
            };
            yield break;
        }
        else if (rawLine.EndsWith("= 1f;"))
        {
            // Hardcoded alpha for networked colors
            yield return new DeserializeInstruction()
            {
                CallType = "1f",
                FieldName = "a"
            };
            yield break;
        }
        else if (rawLine.Contains("@byte &"))
        {
            // Byte shifted flags
            var eqIdx = rawLine.IndexOf('=');
            var refField = rawLine[..eqIdx].Trim();
            var deserializeFn = rawLine[(eqIdx + 1)..].Trim();
            
            yield return new DeserializeInstruction()
            {
                CallType = deserializeFn,
                FieldName = refField.Trim('_')
            };
            yield break;
        }
        
        // Other lines not caught by these checks have not been known to be relevant

        yield break;
    }
}