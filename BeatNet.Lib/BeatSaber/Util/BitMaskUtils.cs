using System.Runtime.CompilerServices;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Util;

public static class BitMaskUtils
{
    public static bool Contains(this PlayerStateHash playerState, string value)
    {
        const int hashCount = 3;
        const int hashBits = 8;

        var valueHash = value.MurmurHash2();
        return playerState.BloomFilter.Contains(valueHash, hashCount, hashBits);
    }
    
    public static bool Contains(this BitMask128 bitMask, string value, int hashCount = 3, int hashBits = 8) =>
        bitMask.Contains(value.MurmurHash2(), hashCount, hashBits);
    
    public static bool Contains(this BitMask256 bitMask, string value, int hashCount = 3, int hashBits = 8) =>
        bitMask.Contains(value.MurmurHash2(), hashCount, hashBits);
    
    public static bool Contains(this BitMask128 bitMask, uint hash, int hashCount = 3, int hashBits = 8)
    {
        for (var i = 0; i < hashCount; i++)
        {
            if (bitMask.GetBits((int)(hash % (ulong)128), 1) == 0UL)
            {
                return false;
            }
            hash >>= hashBits;
        }
        return true;
    }
    
    public static ulong GetBits(this BitMask128 bitMask, int offset, int count)
    {
        var num = (1UL << count) - 1UL;
        var num2 = offset - 64;
        return (bitMask.D0.ShiftRight(num2) | bitMask.D1.ShiftRight(offset)) & num;
    }
    
    public static bool Contains(this BitMask256 bitMask, uint hash, int hashCount = 3, int hashBits = 8)
    {
        for (var i = 0; i < hashCount; i++)
        {
            if (bitMask.GetBits((int)(hash % (ulong)256), 1) == 0UL)
            {
                return false;
            }
            hash >>= hashBits;
        }
        return true;
    }
    
    public static ulong GetBits(this BitMask256 bitMask, int offset, int count)
    {
        var num = (1UL << count) - 1UL;
        var num2 = offset - 192;
        var num3 = bitMask.D0.ShiftRight(num2);
        var num4 = offset - 128;
        var num5 = num3 | bitMask.D1.ShiftRight(num4);
        var num6 = offset - 64;
        return (num5 | bitMask.D2.ShiftRight(num6) | bitMask.D3.ShiftRight(offset)) & num;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftLeft(this ulong value, in int shift)
    {
        if (shift < 0)
        {
            var num = -shift;
            return value.ShiftRight(num);
        }
        if (shift < 64)
        {
            return value << shift;
        }
        return 0UL;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ShiftRight(this ulong value, in int shift)
    {
        if (shift < 0)
        {
            var num = -shift;
            return value.ShiftLeft(num);
        }
        if (shift < 64)
        {
            return value >> shift;
        }
        return 0UL;
    }
}