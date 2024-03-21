using System.Runtime.CompilerServices;
using BeatNet.Lib.BeatSaber.Generated.NetSerializable;

namespace BeatNet.Lib.BeatSaber.Util;

public static class BitMaskUtils
{
    #region Creation

    public static BitMask128 CreateBitMask128(IEnumerable<string> strings)
    {
        var bitMask = new BitMask128();
        foreach (var str in strings)
            bitMask.AddEntry(str);
        return bitMask;
    }
    
    public static BitMask256 CreateBitMask256(IEnumerable<string> strings)
    {
        var bitMask = new BitMask256();
        foreach (var str in strings)
            bitMask.AddEntry(str);
        return bitMask;
    }
    
    #endregion

    #region Addition
    
    public static void AddEntry(this BitMask128 bitMask, string value, int hashCount = 3, int hashBits = 8)
        => bitMask.AddEntryHash(value.MurmurHash2(), hashCount, hashBits);
    
    public static void AddEntry(this BitMask256 bitMask, string value, int hashCount = 3, int hashBits = 8)
        => bitMask.AddEntryHash(value.MurmurHash2(), hashCount, hashBits);

    public static void AddEntryHash(this BitMask128 bitMask, uint hash, int hashCount = 3, int hashBits = 8)
    {
        for (var i = 0; i < hashCount; i++)
        {
            bitMask.SetBits((int)(hash % (ulong)BitMask128.BitCount), 1UL);
            hash >>= hashBits;
        }
    }

    public static void AddEntryHash(this BitMask256 bitMask, uint hash, int hashCount = 3, int hashBits = 8)
    {
        for (var i = 0; i < hashCount; i++)
        {
            bitMask.SetBits((int)(hash % (ulong)BitMask256.BitCount), 1UL);
            hash >>= hashBits;
        }
    }

    #endregion
    
    #region Contains
    
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
    
    #endregion

    #region SetBits
    
    public static void SetBits(this BitMask128 bitMask, int offset, ulong bits)
    {
        bitMask.D0 |= bits.ShiftLeft(offset - 64);
        bitMask.D1 |= bits.ShiftLeft(offset);
    }

    public static void SetBits(this BitMask256 bitMask, int offset, ulong bits)
    {
        bitMask.D0 |= bits.ShiftLeft(offset - 192);
        bitMask.D1 |= bits.ShiftLeft(offset - 128);
        bitMask.D2 |= bits.ShiftLeft(offset - 64);
        bitMask.D3 |= bits.ShiftLeft(offset);
    }

    #endregion
    
    #region GetBits
    
    public static ulong GetBits(this BitMask128 bitMask, int offset, int count)
    {
        var num = (1UL << count) - 1UL;
        var num2 = offset - 64;
        return (bitMask.D0.ShiftRight(num2) | bitMask.D1.ShiftRight(offset)) & num;
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
    
    #endregion
    
    #region ulong shift
    
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
    
    #endregion

    #region PlayerStateHash helpers
    
    public static void Add(this PlayerStateHash playerState, string value)
        => playerState.BloomFilter.AddEntry(value);
    
    public static bool Contains(this PlayerStateHash playerState, string value)
        => playerState.BloomFilter.Contains(value.MurmurHash2());

    #endregion
}