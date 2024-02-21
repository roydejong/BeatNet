using System.Diagnostics;
using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Analysis.Structs;
using BeatNet.CodeGen.Analysis.Util;

namespace BeatNet.CodeGen.Analysis.Domains;

public class ConnectedPlayerManagerAnalyzer : ISubAnalyzer
{
    private PacketResult? _currentPacket = null;
    private DeserializeParser _deserializeParser = new();
    
    public void AnalyzeLine_FirstPass(LineAnalyzer line, Results results)
    {
        if (line.IsClass || line.IsStruct)
        {
            if (line.DeclaredName!.EndsWith("Packet"))
            {
                _currentPacket = new PacketResult()
                {
                    PacketName = line.DeclaredName
                };
                results.Packets.Add(_currentPacket);
            }
            else
            {
                _currentPacket = null;
            }
        }

        if (_currentPacket == null)
            return;
        
        var field = FieldParser.TryParse(line);
        if (field != null)
            _currentPacket.Fields[field.Name] = field;
        
        foreach (var instr in _deserializeParser.FeedNextLine(_currentPacket, line))
            _currentPacket.DeserializeInstructions.Add(instr);
    }

    public void AnalyzeLine_SecondPass(LineAnalyzer line, Results results)
    {
    }
}