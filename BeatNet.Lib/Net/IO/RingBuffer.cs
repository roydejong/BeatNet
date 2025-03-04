using System.Collections.Concurrent;
using Serilog;

namespace BeatNet.Lib.Net.IO;

public class RingBuffer<T>(int capacity) where T : notnull
{
    public readonly int Capacity = capacity;

    private readonly ConcurrentQueue<T> _queue = new();
    private int _atomicCount = 0;

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);

        if (Interlocked.Increment(ref _atomicCount) <= Capacity)
            return;
        
        Log.Logger.Warning("Ring buffer overflow, lobby may be unstable");
        TryDequeue(out _);
    }

    public bool TryDequeue(out T item)
    {
        if (!_queue.TryDequeue(out item!))
            return false;
        
        Interlocked.Decrement(ref _atomicCount);
        return true;
    }
}