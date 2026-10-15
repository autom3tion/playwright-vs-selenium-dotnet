namespace Versus.Bench.Resources;

public sealed class ResourceAccumulator
{
    private readonly Dictionary<int, TimeSpan> _cpu = [];

    /// <summary>
    /// Largest sum of per-process resident memory seen in one sample. Shared pages are counted once per process, so this
    /// is an upper bound. The same limit applies to every tool measured.
    /// </summary>
    public long PeakRssBytes { get; private set; }

    /// <summary>
    /// CPU seconds: each process's last sampled CPU time, summed. A process that exits loses the CPU it used since its
    /// last sample. A helper that starts and exits between two samples (500 ms apart) is missed entirely. Both limits
    /// apply the same way to every tool measured.
    /// </summary>
    public double CpuSeconds => _cpu.Values.Sum(t => t.TotalSeconds);

    public void Add(IReadOnlyList<ProcessSample> tree)
    {
        PeakRssBytes = Math.Max(PeakRssBytes, tree.Sum(p => p.RssBytes));
        foreach (var p in tree)
        {
            _cpu[p.Pid] = _cpu.TryGetValue(p.Pid, out var seen) && seen > p.CpuTime ? seen : p.CpuTime;
        }
    }
}
