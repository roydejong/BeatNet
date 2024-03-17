using System.Diagnostics;

namespace BeatNet.GameServer.Lobby;

public class SyncTimeProvider
{
    private readonly Stopwatch _stopwatch;
    private long _startTicks;
    
    public SyncTimeProvider()
    {
        _stopwatch = new Stopwatch();
        Reset();
    }

    public void Reset()
    {
        _startTicks = GetUtcNowTicks();
        _stopwatch.Start();
    }
    
    public long GetNowTicks() =>
        _startTicks + (long)(_stopwatch.ElapsedTicks * TimeSpanTicksPerStopwatchTick);

    public long GetRunTimeMs() =>
        (GetNowTicks() - _startTicks) / 10000L;

    private static long GetUtcNowTicks() =>
        DateTime.UtcNow.Ticks - Epoch.Ticks;

    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly double TimeSpanTicksPerStopwatchTick = 10000000.0 / Stopwatch.Frequency;
}