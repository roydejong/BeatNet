using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BeatNet.Lib.Net.IO;

public ref struct NetWriter
{
    public readonly Span<byte> Data;
    public int Position { get; private set; }
    
    public NetWriter(Span<byte> data)
    {
        Data = data;
        Position = 0;
    }

    public int RemainingLength => Data.Length - Position;
    
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
            throw new InvalidOperationException($"Write would exceed buffer size (Data.Length={Data.Length}, " +
                                                $"Position={Position}, neededBytes={neededBytes})");
    }
    
    #region Bytes

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte value)
    {
        ThrowIfExceedsBufferSize(1);
        Data[Position++] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        var byteCount = value.Length;
        ThrowIfExceedsBufferSize(byteCount);
        value.CopyTo(Data[Position..]);
        Position += byteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ShiftBytesForward(int fromIndex, int targetIndex, int length)
    {
        if (fromIndex >= targetIndex)
            throw new ArgumentException("fromIndex must be less than targetIndex");

        var finalIndex = targetIndex + length;

        if (finalIndex > Data.Length)
            throw new InvalidOperationException($"Shifted bytes would exceed buffer size (Data.Length={Data.Length}, " +
                                                $"fromIndex={fromIndex}, targetIndex={targetIndex}, length={length})");

        for (var i = length; i >= 0; i--)
        {
            var bFromIndex = fromIndex + i;
            var bToIndex = targetIndex + i;

            Data[bToIndex] = Data[bFromIndex];
        }

        Position = finalIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool value) => WriteByte((byte) (value ? 1 : 0));

    #endregion

    #region Short

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteShort(short value)
    {
        const int byteCount = sizeof(short);
        ThrowIfExceedsBufferSize(byteCount);
        BinaryPrimitives.WriteInt16LittleEndian(Data[Position..], value);
        Position += byteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUShort(ushort value)
    {
        const int byteCount = sizeof(ushort);
        ThrowIfExceedsBufferSize(byteCount);
        BinaryPrimitives.WriteUInt16LittleEndian(Data[Position..], value);
        Position += byteCount;
    }

    #endregion

    #region Int

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt(int value)
    {
        const int byteCount = sizeof(int);
        ThrowIfExceedsBufferSize(byteCount);
        BinaryPrimitives.WriteInt32LittleEndian(Data[Position..], value);
        Position += byteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt(uint value)
    {
        const int byteCount = sizeof(uint);
        ThrowIfExceedsBufferSize(byteCount);
        BinaryPrimitives.WriteUInt32LittleEndian(Data[Position..], value);
        Position += byteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIntEnum<TEnum>(TEnum value) where TEnum : Enum
        => WriteInt(Convert.ToInt32(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteÚIntEnum<TEnum>(TEnum value) where TEnum : Enum
        => WriteUInt(Convert.ToUInt32(value));

    #endregion

    #region Long

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteLong(long value)
    {
        const int byteCount = sizeof(long);
        ThrowIfExceedsBufferSize(byteCount);
        BinaryPrimitives.WriteInt64LittleEndian(Data[Position..], value);
        Position += byteCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteULong(ulong value)
    {
        const int byteCount = sizeof(ulong);
        ThrowIfExceedsBufferSize(byteCount);
        BinaryPrimitives.WriteUInt64LittleEndian(Data[Position..], value);
        Position += byteCount;
    }

    #endregion

    #region Float

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteFloat(float value)
    {
        const int byteCount = sizeof(float);
        ThrowIfExceedsBufferSize(byteCount);
        MemoryMarshal.Write(Data[Position..], ref value);
        Position += byteCount;
    }

    #endregion

    #region String

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string? value)
    {
        var encoding = Encoding.UTF8;

        if (string.IsNullOrEmpty(value))
        {
            WriteInt(0);
            return;
        }

        var byteCount = encoding.GetByteCount(value);
        WriteInt(byteCount);
        WriteBytes(encoding.GetBytes(value));
    }

    #endregion

    #region BeatSaber VarInt

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarULong(ulong value)
    {
        do
        {
            var b = (byte) (value & 127UL);
            value >>= 7;
            if (value != 0UL)
                b |= 128;
            WriteByte(b);
        } while (value != 0UL);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarLong(long value)
        => WriteVarULong((value < 0L ? (ulong) ((-value << 1) - 1L) : (ulong) (value << 1)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarUInt(uint value)
        => WriteVarULong(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteVarInt(int value)
        => WriteVarLong(value);
    
    #endregion
    
    #region BeatSaber Arrays
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByteArray(byte[] value)
    {
        WriteUInt((uint)value.Length);
        WriteBytes(value);
    }
    
    #endregion

    #region Enum

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteEnum<T>(T value) where T : Enum
    {
        throw new NotImplementedException(); // TODO
    }

    #endregion

    #region Serializable

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSerializable<T>(T value) where T : INetSerializable =>
        value.WriteTo(ref this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSerializableList<TList, TItem>(TList value) where TList : IList<TItem> where TItem : INetSerializable
    {
        WriteInt(value.Count);
        foreach (var item in value)
            WriteSerializable(item);
    }

    #endregion
}