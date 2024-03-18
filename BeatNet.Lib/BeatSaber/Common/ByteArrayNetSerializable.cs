using BeatNet.Lib.Net;
using BeatNet.Lib.Net.Interfaces;
using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.BeatSaber.Common;

// ReSharper disable ClassNeverInstantiated.Global
public class ByteArrayNetSerializable : INetSerializable
{
    private byte[]? _data;
    private int _length;
    private readonly string _name;
    private readonly bool _allowEmpty;
    private readonly int _minLength;
    private readonly int _maxLength;

    public ByteArrayNetSerializable(string name, int minLength = 0, int maxLength = 32767, bool allowEmpty = false)
    {
        _data = null;
        _name = name;
        _length = 0;
        _allowEmpty = allowEmpty;
        _minLength = minLength;
        _maxLength = maxLength;
    }

    public void Clear()
    {
        _length = 0;

        // TODO Buffer pool - release
    }

    public byte[]? GetData(bool emptyAsNull = false)
    {
        if (_length == 0)
            return emptyAsNull ? null : Array.Empty<byte>();
        
        var alloc = new byte[_length];
        Array.Copy(_data!, alloc, _length);
        return alloc;
    }

    private void Resize(int length)
    {
        if (_data != null && _length >= length)
            return;

        _length = length;
        _data = new byte[length];

        // TODO Buffer pool - acquire
    }

    public void WriteTo(ref NetWriter writer)
    {
        if (_length == 0 && _minLength > 0 && !_allowEmpty)
            throw new ArgumentException(_name + " is not allowed to be empty");

        if (_minLength != _maxLength || _allowEmpty)
            writer.WriteVarUInt((uint)_length);

        if (_length > 0)
            writer.WriteBytes(_data!, 0, _length);
    }

    public void ReadFrom(ref NetReader reader)
    {
        var num = _minLength;

        if (_minLength != _maxLength || _allowEmpty)
        {
            num = (int)reader.ReadVarUInt();
        }

        if (num == 0)
        {
            if (_minLength > 0 && !_allowEmpty)
                throw new ArgumentException($"{_name} must not be empty");

            Clear();
            return;
        }

        if (num < _minLength || num > _maxLength)
        {
            throw new ArgumentException(
                $"{_name} must be between {_minLength} and {_maxLength} long, given an array of length {_length}");
        }

        Resize(num);
        reader.ReadBytesInto(_data!, 0, _length);
    }
}