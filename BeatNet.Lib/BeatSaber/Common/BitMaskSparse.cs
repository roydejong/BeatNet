using BeatNet.Lib.Net;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Common;

// ReSharper disable ClassNeverInstantiated.Global

public class BitMaskSparse : INetSerializable
{
    public readonly int BitCount;
    private readonly SortedSet<uint> _sparseSet;
    
    public BitMaskSparse(int bitCount)
    {
        BitCount = bitCount;
        _sparseSet = new();
    }
    
    public void WriteTo(ref NetWriter writer)
    {
        var num = 0U;
        
        foreach (var num2 in _sparseSet)
        {
            writer.WriteVarUInt(num2 - num);
            num = num2;
        }
        
        writer.WriteVarUInt((uint)(BitCount - num));
    }

    public void ReadFrom(ref NetReader reader)
    {
        var num = 0U;
        
        while (true)
        {
            var varUInt = reader.ReadVarUInt();
            num += varUInt;
            
            if (num >= (ulong)BitCount)
                break;
            
            _sparseSet.Add(num);
        }
    }
}