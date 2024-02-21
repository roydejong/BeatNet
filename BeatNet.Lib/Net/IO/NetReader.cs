using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BeatNet.Lib.BeatSaber.Common;

namespace BeatNet.Lib.Net.IO;

public ref struct NetReader
{
    public readonly ReadOnlySpan<byte> Data;
    public int Position { get; private set; }
    
    public NetReader(ReadOnlySpan<byte> data)
    {
        Data = data;
        Position = 0;
    }

    public int RemainingLength => Data.Length - Position;
    public bool EndOfData => RemainingLength <= 0;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        Position = 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int skipBytes)
    {
        ThrowIfExceedsBufferSize(skipBytes);
        Position += skipBytes;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfExceedsBufferSize(int neededBytes)
    {
        if (Position + neededBytes > Data.Length)
            throw new InvalidOperationException($"Read would exceed buffer size (Data.Length={Data.Length}, " +
                                                $"Position={Position}, neededBytes={neededBytes})");
    }
    
    #region Bytes

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        ThrowIfExceedsBufferSize(1);
        return Data[Position++];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadBytes(int byteCount)
    {
        ThrowIfExceedsBufferSize(byteCount);
        var value = Data[Position..(Position + byteCount)];
        Position += byteCount;
        return value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool() => ReadByte() == 1;

    #endregion

    #region Short

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadShort()
    {
        const int byteCount = sizeof(short);
        ThrowIfExceedsBufferSize(byteCount);
        var value = BinaryPrimitives.ReadInt16LittleEndian(Data[Position..]);
        Position += byteCount;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUShort()
    {
        const int byteCount = sizeof(ushort);
        ThrowIfExceedsBufferSize(byteCount);
        var value = BinaryPrimitives.ReadUInt16LittleEndian(Data[Position..]);
        Position += byteCount;
        return value;
    }

    #endregion
    
    #region Int

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt()
    {
        const int byteCount = sizeof(int);
        ThrowIfExceedsBufferSize(byteCount);
        var value = BinaryPrimitives.ReadInt32LittleEndian(Data[Position..]);
        Position += byteCount;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt()
    {
        const int byteCount = sizeof(uint);
        ThrowIfExceedsBufferSize(byteCount);
        var value = BinaryPrimitives.ReadUInt32LittleEndian(Data[Position..]);
        Position += byteCount;
        return value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEnum ReadIntEnum<TEnum>() where TEnum : Enum
        => (TEnum)Enum.ToObject(typeof(TEnum), ReadInt());
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TEnum ReadUIntEnum<TEnum>() where TEnum : Enum
        => (TEnum)Enum.ToObject(typeof(TEnum), ReadUInt());

    #endregion
    
    #region Long

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadLong()
    {
        const int byteCount = sizeof(long);
        ThrowIfExceedsBufferSize(byteCount);
        var value = BinaryPrimitives.ReadInt64LittleEndian(Data[Position..]);
        Position += byteCount;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadULong()
    {
        const int byteCount = sizeof(ulong);
        ThrowIfExceedsBufferSize(byteCount);
        var value = BinaryPrimitives.ReadUInt64LittleEndian(Data[Position..]);
        Position += byteCount;
        return value;
    }

    #endregion

    #region Float

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat()
    {
        const int byteCount = sizeof(float);
        ThrowIfExceedsBufferSize(byteCount);
        var value = MemoryMarshal.Read<float>(Data[Position..]);
        Position += byteCount;
        return value;
    }
    
    #endregion
    
    #region String

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadString()
    {
        var byteCount = ReadInt();
        if (byteCount > 0)
            return Encoding.UTF8.GetString(ReadBytes(byteCount));
        return "";
    }

    public bool TryReadString([NotNullWhen(true)] out string? result)
    {
        result = null;
        
        if (RemainingLength < 4)
            // Not enough for length prefix
            return false;
        
        var byteCount = ReadInt();
        
        if (byteCount <= 0)
        {
            result = "";
            return true;
        }
        
        if (byteCount > RemainingLength)
            // Not enough for string
            return false;
        
        result = Encoding.UTF8.GetString(ReadBytes(byteCount));
        return true;
    }

    #endregion
    
    #region BeatSaber Var

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadVarULong()
    {
        var value = 0UL;
        var shift = 0;
        var b = ReadByte();
        while ((b & 128UL) != 0UL)
        {
            value |= (b & 127UL) << shift;
            shift += 7;
            b = ReadByte();
        }
        return value | (ulong)b << shift;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadVarLong()
    {
        var varULong = (long)ReadVarULong();
        if ((varULong & 1L) != 1L)
            return varULong >> 1;
        return -(varULong >> 1) + 1L;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadVarUInt()
        => (uint)ReadVarULong();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadVarInt()
        => (int)ReadVarLong();

    #endregion

    #region BeatSaber Arrays
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadByteArray()
    {
        var byteCount = ReadUShort();
        ThrowIfExceedsBufferSize(byteCount);
        var slice = Data[Position..(Position + byteCount)];
        Position += byteCount;
        return slice.ToArray();
    }
    
    #endregion
    
    #region Enum

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadEnum<T>() where T : Enum
    {
        throw new NotImplementedException(); // TODO
    }

    #endregion

    #region Serializable

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadSerializable<T>() where T : INetSerializable
    {
        var instance = (T)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(T));
        instance.ReadFrom(ref this);
        return instance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TList ReadSerializableList<TList, TItem>() where TList : IList<TItem> where TItem : INetSerializable
    {
        var count = ReadInt();
        var instance = (TList)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TList));
        for (var i = 0; i < count; i++)
            instance.Add(ReadSerializable<TItem>());
        return instance;
    }

    #endregion
}