using BeatNet.Lib.Net;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Common;

// ReSharper disable ClassNeverInstantiated.Global
public class BitMaskArray : INetSerializable
{
    public readonly int BitCount;
    private readonly ulong[] _data;

    public BitMaskArray(int bitCount)
    {
        BitCount = bitCount;
        _data = new ulong[(bitCount + 63) / 64];
    }

    public void WriteTo(ref NetWriter writer)
    {
        foreach (var t in _data)
        {
            byte b = 0;
            for (var j = 0; j < 8; j++)
            {
                if ((t >> 8 * j & 255UL) != 0UL)
                {
                    b |= (byte)(1 << j);
                }
            }

            writer.WriteByte(b);
            
            for (var k = 0; k < 8; k++)
            {
                if ((t >> 8 * k & 255UL) != 0UL)
                {
                    writer.WriteByte((byte)(t >> 8 * k & 255UL));
                }
            }
        }
    }

    public void ReadFrom(ref NetReader reader)
    {
        for (var i = 0; i < _data.Length; i++)
        {
            var @byte = reader.ReadByte();
            
            for (var j = 0; j < 8; j++)
            {
                if ((@byte & 1 << j) == 0)
                    continue;
                
                var byte2 = reader.ReadByte();
                _data[i] |= (ulong)byte2 << j * 8;
            }
        }
    }
}