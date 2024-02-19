using System.Diagnostics;
using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Analysis.Util;

public class DeserializeParser
{
    private bool _inDeserialize = false;
    private int _deserializeDepth = 0;
    
    public DeserializeInstruction? FeedNextLine(LineAnalyzer line)
    {
        if (!_inDeserialize)
        {
            _deserializeDepth = 0;
            
            if (line.IsMethod && line.DeclaredName!.Contains("Deserialize"))
                _inDeserialize = true;
            
            return null;
        }

        if (line.IsOpenBracket)
        {
            _deserializeDepth++;
            return null;
        } 
        else if (line.IsCloseBracket)
        {
            _deserializeDepth--;
            return null;
        }

        if (_deserializeDepth == 0)
        {
            _inDeserialize = false;
            return null;
        }

        var rawLine = line.RawLine;
        rawLine = rawLine.Replace("this.", "");
        rawLine = rawLine.Replace("_color.", "");

        if (rawLine.Contains("reader."))
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
            
            return new DeserializeInstruction()
            {
                CallType = callType,
                FieldName = refField.Trim('_'),
                TypeCast = typeCast
            };
        }
        else if (rawLine.Contains(".Deserialize"))
        {
            // Deserialize assign
            var hasEq = rawLine.Contains('=');

            if (hasEq)
            {
                // Assign the result of Object.Deserialize()
                var eqIdx = rawLine.IndexOf('=');
                var refField = rawLine[..eqIdx].Trim();
                var deserializeFn = rawLine[(eqIdx + 1)..].Trim();
            
                return new DeserializeInstruction()
                {
                    CallType = deserializeFn,
                    FieldName = refField.Trim('_')
                };
            }
            else
            {
                // Object.Deserialize() fills existing struct / no explicit assignment
                var dotIdx = rawLine.IndexOf(".Deserialize", StringComparison.Ordinal);
                var refField = rawLine[..dotIdx];
            
                return new DeserializeInstruction()
                {
                    CallType = "Deserialize();",
                    FieldName = refField.Trim('_')
                };
            }
        }
        else if (rawLine.EndsWith("= 1f;"))
        {
            // Hardcoded alpha for networked colors
            return new DeserializeInstruction()
            {
                CallType = "1f",
                FieldName = "a"
            };
        }
        else if (rawLine.Contains("@byte &"))
        {
            // Byte shifted flags
            var eqIdx = rawLine.IndexOf('=');
            var refField = rawLine[..eqIdx].Trim();
            var deserializeFn = rawLine[(eqIdx + 1)..].Trim();
            
            return new DeserializeInstruction()
            {
                CallType = deserializeFn,
                FieldName = refField.Trim('_')
            };
        }
        else
        {
            Debugger.Break();
        }

        return null;
    }
}