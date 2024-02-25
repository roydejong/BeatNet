namespace BeatNet.GameServer.Management;

public class PortAllocator
{
    public readonly ushort Min;
    public readonly ushort Max;

    private readonly Queue<ushort> _availablePorts;

    public bool IsEmpty => _availablePorts.Count == 0;
    public int MaxCapacity => Max - Min + 1;
    
    public PortAllocator(ushort min, ushort max)
    {
        Min = min;
        Max = max;
        
        if (MaxCapacity < 1)
            throw new ArgumentOutOfRangeException(nameof(MaxCapacity), "Port range is invalid.");
        
        _availablePorts = new(MaxCapacity);
        Reset();
    }

    public void Reset()
    {
        _availablePorts.Clear();
        
        for (var i = Min; i <= Max; i++)
            _availablePorts.Enqueue(i);
    }

    public void Return(ushort portNumber)
    {
        if (portNumber < Min || portNumber > Max)
            throw new ArgumentOutOfRangeException(nameof(portNumber), "Port number is out of range.");
        
        if (_availablePorts.Contains(portNumber))
            return;

        _availablePorts.Enqueue(portNumber);
    }
    
    public bool TryAcquire(out ushort portNumber)
    {
        return _availablePorts.TryDequeue(out portNumber);
    }
}