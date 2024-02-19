namespace BeatNet.Lib.BeatSaber.Rpc;

public abstract class BaseRpc
{
    public abstract byte RpcType { get; }
    
    public long SyncTime { get; set; }

    public abstract int ValueCount { get; }
    public abstract object? Value0 { get; }
    public abstract object? Value1 { get; }
    public abstract object? Value2 { get; }
    public abstract object? Value3 { get; }

    public object? GetValue(int index)
    {
        return index switch
        {
            0 => Value0,
            1 => Value1,
            2 => Value2,
            3 => Value3,
            _ => null
        };
    }

    public void WriteTo(object writer)
    {
        // TODO: Write var ulong SyncTime
    }

    public void ReadFrom(object reader)
    {
        // TODO: Read var ulong SyncTime
    }
}