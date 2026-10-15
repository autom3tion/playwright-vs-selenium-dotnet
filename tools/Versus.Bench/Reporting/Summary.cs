using Versus.Bench.Running;

namespace Versus.Bench.Reporting;

public sealed record Summary(
    EnvInfo Env,
    int Rounds,
    IReadOnlyList<int> Workers,
    int SiteDownReruns,
    int RunCrashes,
    IReadOnlyList<TestStats> PerTest,
    IReadOnlyList<ToolStats> PerTool,
    IReadOnlyList<ScalingPoint> Scaling,
    IReadOnlyList<LocRow> Loc);

public sealed record TestStats(
    string TestKey,
    string Tool,
    int Runs,
    int Passed,
    double PassRate,
    double? MedianBodyMs,
    double? P95BodyMs,
    bool SpeedExcluded,
    IReadOnlyDictionary<string, int> Failures);

/// <summary>SpeedTotalMs: the sum of per-test median body times over the SpeedTests tests both tools have a figure for.</summary>
public sealed record ToolStats(
    string Tool,
    double? MedianSetupMs,
    double? MedianTeardownMs,
    double? MedianLaunchMs,
    double PassRate,
    double SpeedTotalMs,
    int SpeedTests);

public sealed record ScalingPoint(string Tool, int Workers, double MedianWallMs, double MedianPeakRssMb, double MedianCpuPercent);

public sealed record LocRow(string Key, int Playwright, int Selenium);
