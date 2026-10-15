namespace Versus.Bench.Resources;

/// <summary>
/// Samples the process tree under one root on a timer until stopped. The root must be a process started just before
/// Start: CPU time is cumulative per process, so any CPU the tree spent before sampling began would be counted.
/// A snapshot that fails is skipped; sampling continues.
/// </summary>
public sealed class ResourceSampler
{
    private readonly CancellationTokenSource _stop = new();
    private readonly ResourceAccumulator _usage = new();
    private readonly Task _loop;

    private ResourceSampler(int rootPid, TimeSpan interval) => _loop = Task.Run(() => LoopAsync(rootPid, interval));

    public static ResourceSampler Start(int rootPid, TimeSpan interval) => new(rootPid, interval);

    public async Task<ResourceAccumulator> StopAsync()
    {
        await _stop.CancelAsync();
        await _loop;
        return _usage;
    }

    private async Task LoopAsync(int rootPid, TimeSpan interval)
    {
        using var timer = new PeriodicTimer(interval);
        try
        {
            do
            {
                try
                {
                    _usage.Add(ProcessTree.Of(await ProcessTable.SnapshotAsync(), rootPid));
                }
                catch (BenchException)
                {
                    // A transient failure of one snapshot must not end sampling for the whole run.
                }
            }
            while (await timer.WaitForNextTickAsync(_stop.Token));
        }
        catch (OperationCanceledException)
        {
            // Stopped.
        }
    }
}
