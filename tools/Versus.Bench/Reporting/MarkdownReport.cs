using System.Globalization;
using System.Text;
using Versus.Timing;

namespace Versus.Bench.Reporting;

public static class MarkdownReport
{
    public const string SpeedExcludedMark = "†";

    public static string ForReadme(Summary s, string resultsPath)
    {
        var b = new StringBuilder();
        b.AppendLine($"**{Headline.Build(s)}**");
        b.AppendLine();
        b.AppendLine(Invariant(
            $"_{Format.Date(s.Env.DateUtc)} · {s.Env.Cpu} · {s.Env.Os} · Chrome for Testing {s.Env.ChromeVersion} · {Format.Plural(s.Rounds, "round")} at {string.Join("/", s.Workers)} {(s.Workers.Count == 1 && s.Workers[0] == 1 ? "worker" : "workers")} · [full report]({resultsPath}/report.md)_"));
        b.AppendLine();
        AppendSummaryTable(b, s);
        b.AppendLine();
        AppendPassRateTable(b, s);
        b.AppendLine();
        AppendCharts(b, $"{resultsPath}/charts");
        return b.ToString();
    }

    public static string Full(Summary s)
    {
        var b = new StringBuilder();
        b.AppendLine($"# Playwright vs Selenium — {Format.Date(s.Env.DateUtc)}, {s.Env.Machine}");
        b.AppendLine();
        b.AppendLine($"**{Headline.Build(s)}**");
        b.AppendLine();

        b.AppendLine("## Environment");
        b.AppendLine();
        b.AppendLine(Invariant($"- {s.Env.Os}; {s.Env.Cpu}, {s.Env.Cores} cores; {s.Env.RamBytes / (1L << 30)} GB RAM"));
        b.AppendLine($"- {s.Env.DotnetRuntime}, SDK {s.Env.DotnetSdk}; Chrome for Testing {s.Env.ChromeVersion}");
        b.AppendLine($"- Packages: {string.Join(", ", s.Env.Packages.Select(p => $"{p.Key} {p.Value}"))}");
        b.AppendLine($"- Median latency: {string.Join(", ", s.Env.LatencyMs.Select(l => $"{l.Key} {Format.Ms(l.Value)}"))}");
        b.AppendLine(Invariant($"- {Format.Plural(s.Rounds, "measured round")} at {string.Join(", ", s.Workers)} {(s.Workers.Count == 1 && s.Workers[0] == 1 ? "worker" : "workers")}; {Format.Plural(s.SiteDownReruns, "round")} re-run because a site was down; {s.RunCrashes} run {(s.RunCrashes == 1 ? "crash" : "crashes")}"));
        b.AppendLine();

        b.AppendLine("## Summary");
        b.AppendLine();
        AppendSummaryTable(b, s);
        b.AppendLine();

        b.AppendLine("## Per test");
        b.AppendLine();
        b.AppendLine("| Test | Playwright median | p95 | pass | Selenium median | p95 | pass | Failures |");
        b.AppendLine("|---|---:|---:|---:|---:|---:|---:|---|");
        foreach (var key in s.PerTest.Select(t => t.TestKey).Distinct())
        {
            var pw = s.PerTest.SingleOrDefault(t => t.TestKey == key && t.Tool == Tools.Playwright);
            var se = s.PerTest.SingleOrDefault(t => t.TestKey == key && t.Tool == Tools.Selenium);
            var mark = (pw ?? se)!.SpeedExcluded ? $" {SpeedExcludedMark}" : "";
            b.AppendLine($"| {key}{mark} | {Format.Ms(pw?.MedianBodyMs)} | {Format.Ms(pw?.P95BodyMs)} | {Pass(pw)} | " +
                         $"{Format.Ms(se?.MedianBodyMs)} | {Format.Ms(se?.P95BodyMs)} | {Pass(se)} | {Failures(pw, se)} |");
        }

        b.AppendLine();
        if (s.PerTest.Any(t => t.SpeedExcluded))
        {
            b.AppendLine($"{SpeedExcludedMark} Left out of speed figures: Playwright's trial click waits 1 s on purpose, so its time measures the test's design, not the tool.");
            b.AppendLine();
        }

        b.AppendLine("Speed figures use passed runs at 1 worker. Pass rates use every run at every worker count. " +
                     "A test that ran but left no timing record counts as a failed run of type `harness`; a run that crashed twice " +
                     "with no TRX is counted under run crashes and its tests are missing from that round.");
        b.AppendLine();

        b.AppendLine("## Parallel scaling");
        b.AppendLine();
        b.AppendLine("| Tool | Workers | Suite time (median) | Peak memory (median) | CPU (median, % of one core) |");
        b.AppendLine("|---|---:|---:|---:|---:|");
        foreach (var p in s.Scaling)
        {
            b.AppendLine(Invariant($"| {p.Tool} | {p.Workers} | {Format.Ms(p.MedianWallMs)} | {Format.Mb(p.MedianPeakRssMb)} | {p.MedianCpuPercent:0}% |"));
        }

        b.AppendLine();
        b.AppendLine("## Lines of code");
        b.AppendLine();
        b.AppendLine("| Test | Playwright | Selenium |");
        b.AppendLine("|---|---:|---:|");
        foreach (var row in s.Loc)
        {
            b.AppendLine(Invariant($"| {row.Key} | {row.Playwright} | {row.Selenium} |"));
        }

        b.AppendLine();
        b.AppendLine("## Charts");
        b.AppendLine();
        AppendCharts(b, "charts");
        return b.ToString();
    }

    private static void AppendSummaryTable(StringBuilder b, Summary s)
    {
        var pw = s.PerTool.Single(t => t.Tool == Tools.Playwright);
        var se = s.PerTool.Single(t => t.Tool == Tools.Selenium);
        var workers = s.Workers.Count > 0 ? s.Workers.Min() : 1;
        var pwMb = s.Scaling.SingleOrDefault(p => p.Tool == Tools.Playwright && p.Workers == workers)?.MedianPeakRssMb;
        var seMb = s.Scaling.SingleOrDefault(p => p.Tool == Tools.Selenium && p.Workers == workers)?.MedianPeakRssMb;
        var testRows = s.Loc.Where(r => r.Key != LocCounter.FixtureKey).ToList();

        b.AppendLine("| | Playwright | Selenium | Better |");
        b.AppendLine("|---|---:|---:|---|");
        var comparable = pw.SpeedTests > 0 && se.SpeedTests > 0;
        b.AppendLine(Invariant($"| Test time, sum of per-test medians ({pw.SpeedTests} tests, 1 worker) | {Format.Ms(comparable ? pw.SpeedTotalMs : null)} | {Format.Ms(comparable ? se.SpeedTotalMs : null)} | {(comparable ? Lower(pw.SpeedTotalMs, se.SpeedTotalMs) : "")} |"));
        b.AppendLine($"| Setup per test, median | {Format.Ms(pw.MedianSetupMs)} | {Format.Ms(se.MedianSetupMs)} | {Lower(pw.MedianSetupMs, se.MedianSetupMs)} |");
        b.AppendLine($"| Browser launch, once per run | {Format.Ms(pw.MedianLaunchMs)} | {Format.Ms(se.MedianLaunchMs)} | |");
        b.AppendLine($"| Pass rate, every run | {Format.Percent(pw.PassRate)} | {Format.Percent(se.PassRate)} | {HigherPass(pw.PassRate, se.PassRate)} |");
        b.AppendLine(Invariant($"| Peak memory at {Format.Plural(workers, "worker")}, median | {Format.Mb(pwMb)} | {Format.Mb(seMb)} | {Lower(pwMb, seMb)} |"));
        b.AppendLine(Invariant($"| Lines of code, all tests | {testRows.Sum(r => r.Playwright)} | {testRows.Sum(r => r.Selenium)} | {Lower(testRows.Sum(r => r.Playwright), testRows.Sum(r => r.Selenium))} |"));
    }

    /// <summary>Spec §7: pass rates are a table, tests × tools, rather than a chart.</summary>
    private static void AppendPassRateTable(StringBuilder b, Summary s)
    {
        b.AppendLine("| Pass rate per test | Playwright | Selenium |");
        b.AppendLine("|---|---:|---:|");
        foreach (var key in s.PerTest.Select(t => t.TestKey).Distinct())
        {
            var pw = s.PerTest.SingleOrDefault(t => t.TestKey == key && t.Tool == Tools.Playwright);
            var se = s.PerTest.SingleOrDefault(t => t.TestKey == key && t.Tool == Tools.Selenium);
            b.AppendLine($"| {key} | {Pass(pw)} | {Pass(se)} |");
        }
    }

    private static void AppendCharts(StringBuilder b, string chartPath)
    {
        foreach (var (file, alt) in ChartWriter.Charts)
        {
            b.AppendLine("<picture>");
            b.AppendLine($"  <source media=\"(prefers-color-scheme: dark)\" srcset=\"{chartPath}/{file}.dark.svg\">");
            b.AppendLine($"  <img alt=\"{alt}\" src=\"{chartPath}/{file}.light.svg\">");
            b.AppendLine("</picture>");
            b.AppendLine();
        }
    }

    private static string Lower(double? pw, double? se)
    {
        if (pw is null || se is null)
        {
            return "";
        }

        return Stats.IsTie(pw.Value, se.Value) ? "tie" : pw < se ? Tools.Playwright : Tools.Selenium;
    }

    private static string HigherPass(double pw, double se) =>
        Math.Abs(pw - se) * 100 < Headline.PassRateTiePoints ? "tie" : pw > se ? Tools.Playwright : Tools.Selenium;

    private static string Pass(TestStats? t) => t is null ? Format.Missing : Format.Percent(t.PassRate);

    private static string Failures(TestStats? pw, TestStats? se)
    {
        var parts = new[] { pw, se }
            .Where(t => t is not null && t.Failures.Count > 0)
            .Select(t => $"{t!.Tool}: {string.Join(", ", t.Failures.Select(f => $"{f.Key} ×{f.Value}"))}");
        return string.Join("; ", parts);
    }

    private static string Invariant(FormattableString s) => s.ToString(CultureInfo.InvariantCulture);
}
