using Versus.Timing;

namespace Versus.Bench.Reporting;

public static class Aggregator
{
    /// <summary>Speed is compared without parallel contention.</summary>
    public const int SpeedWorkers = 1;

    /// <summary>Tests whose body time measures the test's design rather than the tool. Footnoted in the report.</summary>
    public static readonly IReadOnlySet<string> SpeedExcluded = new HashSet<string>
    {
        "Flakiness.HiddenLayersTests.GreenButtonCannotBeClickedTwice",
    };

    public static Summary Summarize(RawData data, IReadOnlyList<LocRow> loc)
    {
        var tests = data.Timings.Where(t => t.TestKey != TimingRecord.LaunchKey).ToList();

        var perTest = tests
            .GroupBy(t => (t.TestKey, t.Tool))
            .OrderBy(g => g.Key.TestKey, StringComparer.Ordinal).ThenBy(g => g.Key.Tool, StringComparer.Ordinal)
            .Select(g => ForTest(g.Key.TestKey, g.Key.Tool, g.ToList()))
            .ToList();

        var perTool = Tools.All.Select(tool => ForTool(tool, data, tests, perTest)).ToList();

        var scaling = data.Runs
            .Where(r => !r.Crashed)
            .GroupBy(r => (r.Tool, r.Workers))
            .OrderBy(g => g.Key.Tool, StringComparer.Ordinal).ThenBy(g => g.Key.Workers)
            .Select(g => new ScalingPoint(
                g.Key.Tool,
                g.Key.Workers,
                Stats.Median(g.Select(r => r.WallMs)),
                Stats.Median(g.Select(r => r.PeakRssBytes / 1048576.0)),
                Stats.Median(g.Select(r => r.CpuSeconds / (r.WallMs / 1000) * 100))))
            .ToList();

        return new Summary(
            data.Env,
            data.Runs.Select(r => r.Round).Distinct().Count(),
            data.Runs.Select(r => r.Workers).Distinct().Order().ToList(),
            data.Log.SiteDownReruns,
            data.Runs.Count(r => r.Crashed),
            perTest,
            perTool,
            scaling,
            loc);
    }

    private static TestStats ForTest(string key, string tool, IReadOnlyList<TimingRecord> runs)
    {
        var passed = runs.Count(r => r.Outcome == "Passed");
        var excluded = SpeedExcluded.Contains(key);
        var speed = excluded
            ? []
            : runs.Where(r => r.Outcome == "Passed" && r.Workers == SpeedWorkers).Select(r => r.BodyMs).ToList();

        return new TestStats(
            key,
            tool,
            runs.Count,
            passed,
            (double)passed / runs.Count,
            speed.Count > 0 ? Stats.Median(speed) : null,
            speed.Count > 0 ? Stats.Percentile(speed, 0.95) : null,
            excluded,
            runs.Where(r => r.FailureType is not null).GroupBy(r => r.FailureType!).ToDictionary(g => g.Key, g => g.Count()));
    }

    private static ToolStats ForTool(string tool, RawData data, IReadOnlyList<TimingRecord> tests, IReadOnlyList<TestStats> perTest)
    {
        var all = tests.Where(t => t.Tool == tool).ToList();
        var timed = all.Where(t => t.FailureType != FailureType.Harness).ToList();
        var launches = data.Timings.Where(t => t.Tool == tool && t.TestKey == TimingRecord.LaunchKey).Select(t => t.SetupMs).ToList();

        // Only tests where both tools have a speed figure, so neither total is padded by tests the other failed.
        var common = perTest
            .GroupBy(s => s.TestKey)
            .Where(g => g.Count(s => s.MedianBodyMs is not null) == Tools.All.Count)
            .Select(g => g.Key)
            .ToHashSet();
        var speed = perTest.Where(s => s.Tool == tool && common.Contains(s.TestKey)).ToList();

        return new ToolStats(
            tool,
            MedianOrNull(timed.Select(t => t.SetupMs)),
            MedianOrNull(timed.Select(t => t.TeardownMs)),
            MedianOrNull(launches),
            all.Count == 0 ? 0 : (double)all.Count(t => t.Outcome == "Passed") / all.Count,
            speed.Sum(s => s.MedianBodyMs!.Value),
            speed.Count);
    }

    private static double? MedianOrNull(IEnumerable<double> values)
    {
        var list = values.ToList();
        return list.Count > 0 ? Stats.Median(list) : null;
    }
}
