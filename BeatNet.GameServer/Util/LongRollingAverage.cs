using System;

namespace BeatNet.GameServer.Util;

public class LongRollingAverage
{
    private long _currentTotal;
    private long _currentAverage;
    private readonly long[] _buffer;
    private int _index;
    private int _length;

    public long CurrentAverage => _currentAverage;

    public bool HasValue => _length > 0;

    public LongRollingAverage(int windowSize)
    {
        _buffer = new long[windowSize];
    }

    public void Update(long value)
    {
        if (_length == _buffer.Length)
        {
            _currentTotal -= _buffer[_index];
        }
        _buffer[_index] = value;
        _index = (_index + 1) % _buffer.Length;
        _length = Math.Min(_length + 1, _buffer.Length);
        _currentTotal += value;
        _currentAverage = _currentTotal / _length;
    }

    public void Reset()
    {
        _currentAverage = 0L;
        _index = 0;
        _length = 0;
        _currentTotal = 0L;
    }
}