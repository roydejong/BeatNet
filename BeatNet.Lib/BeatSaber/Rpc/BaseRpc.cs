namespace BeatNet.Lib.BeatSaber.Rpc;

public abstract class BaseRpc
{
    public long SyncTime { get; set; }

    public object? Param0 { get; } = null;
    public object? Param1 { get; } = null;
    public object? Param2 { get; } = null;
    public object? Param3 { get; } = null;

    public int ParamCount => 0;

    public void WriteTo(object writer)
    {
        // TODO: Write var ulong SyncTime
    }

    public void ReadFrom(object reader)
    {
        // TODO: Read var ulong SyncTime
    }
}