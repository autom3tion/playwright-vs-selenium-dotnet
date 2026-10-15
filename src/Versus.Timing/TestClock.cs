namespace Versus.Timing;

/// <summary>
/// Marks the four moments of a test: start, setup done (end of [BeforeTest]), body done (start of
/// [AfterTest]) and teardown done (when Finish is called). Test bodies hold no timing code.
/// </summary>
public sealed class TestClock(TimeProvider time)
{
    private long _start;
    private long _setupDone;
    private long _bodyDone;

    public DateTimeOffset StartedUtc { get; private set; }

    public void Start()
    {
        StartedUtc = time.GetUtcNow();
        _start = _setupDone = _bodyDone = time.GetTimestamp();
    }

    public void SetupDone() => _setupDone = time.GetTimestamp();

    public void BodyDone() => _bodyDone = time.GetTimestamp();

    public (double SetupMs, double BodyMs, double TeardownMs) Finish()
    {
        var end = time.GetTimestamp();
        return (Ms(_start, _setupDone), Ms(_setupDone, _bodyDone), Ms(_bodyDone, end));
    }

    private double Ms(long from, long to) => time.GetElapsedTime(from, to).TotalMilliseconds;
}
