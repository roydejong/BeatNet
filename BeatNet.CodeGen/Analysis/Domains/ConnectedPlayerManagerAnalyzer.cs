using BeatNet.CodeGen.Analysis.ResultData;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.Domains;

public class ConnectedPlayerManagerAnalyzer : ISubAnalyzer
{
    private PacketResult? _currentPacket = null;
    
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

        if (line.IsField)
        {
            var name = line.DeclaredName!;
            name = name.Trim('_');

            if (name.Contains("__BackingField"))
                return;
            
            _currentPacket.Fields[name] = new TypedParam()
            {
                TypeName = line.DeclaredType!,
                ParamName = name
            };
        }
    }

    public void AnalyzeLine_SecondPass(LineAnalyzer line, Results results)
    {
        
    }
}