namespace Versus.Bench.Running;

/// <summary>One run of one test project. Crashed: it exited with neither 0 nor 2, or wrote no TRX file.</summary>
public sealed record RunRecord(
    string Tool, int Round, int Workers, double WallMs, long PeakRssBytes, double CpuSeconds, int ExitCode, bool Crashed, int Attempts);

public sealed record BenchLog(int SiteDownReruns);
