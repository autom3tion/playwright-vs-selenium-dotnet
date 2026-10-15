using Precept;
using Versus.Bench.Reporting;

namespace Versus.Bench.Tests.Bench;

[TestSuite("MarkdownReport")]
public class MarkdownReportTests
{
    [Test("The full report marks the test left out of speed and explains why")]
    public async Task SpeedExcludedFootnote()
    {
        var report = MarkdownReport.Full(Samples.Summary());

        await Assert.That(report).ToContainAsync("Flakiness.HiddenLayersTests.GreenButtonCannotBeClickedTwice †");
        await Assert.That(report).ToContainAsync("† Left out of speed figures");
    }

    [Test("A missing figure prints as a dash, never as 0")]
    public async Task MissingFigures() =>
        await Assert.That(MarkdownReport.Full(Samples.Summary())).ToContainAsync("| — |");

    [Test("Failures are listed by type")]
    public async Task Failures() =>
        await Assert.That(MarkdownReport.Full(Samples.Summary())).ToContainAsync("timeout ×1");

    [Test("The README block links the full report and both chart themes")]
    public async Task ReadmeLinks()
    {
        var block = MarkdownReport.ForReadme(Samples.Summary(), "results/2026-10-20-test-machine");

        await Assert.That(block).ToContainAsync("(results/2026-10-20-test-machine/report.md)");
        await Assert.That(block).ToContainAsync("results/2026-10-20-test-machine/charts/body-time.dark.svg");
        await Assert.That(block).ToContainAsync("results/2026-10-20-test-machine/charts/body-time.light.svg");
    }

    [Test("Without a left-out test there is no dagger footnote")]
    public async Task NoFootnoteWhenNothingExcluded()
    {
        var s = Samples.Summary();
        var none = s with { PerTest = s.PerTest.Select(t => t with { SpeedExcluded = false }).ToList() };

        await Assert.That(MarkdownReport.Full(none).Contains("Left out of speed figures", StringComparison.Ordinal)).ToBeFalseAsync();
    }

    [Test("A tool with no timed tests prints dashes and no winner for test time")]
    public async Task UncomparableSpeed()
    {
        var s = Samples.Summary();
        var none = s with { PerTool = s.PerTool.Select(t => t.Tool == "Selenium" ? t with { SpeedTests = 0, SpeedTotalMs = 0 } : t).ToList() };

        await Assert.That(MarkdownReport.Full(none)).ToContainAsync("(21 tests, 1 worker) | — | — |  |");
    }

    [Test("A single round at one worker reads in the singular")]
    public async Task SingularGrammar()
    {
        var one = Samples.Summary() with { Rounds = 1, Workers = [1] };
        var report = MarkdownReport.Full(one);

        await Assert.That(report).ToContainAsync("1 measured round at 1 worker;");
        await Assert.That(report).ToContainAsync("Peak memory at 1 worker, median");
        await Assert.That(MarkdownReport.ForReadme(one, "results/x")).ToContainAsync("1 round at 1 worker ·");
        await Assert.That(MarkdownReport.Full(Samples.Summary())).ToContainAsync("20 measured rounds at 1, 2, 4 workers;");
    }

    [Test("A site with no latency sample prints as a dash")]
    public async Task NegativeLatency()
    {
        var s = Samples.Summary();
        var down = s with { Env = s.Env with { LatencyMs = new Dictionary<string, double> { ["http://a/"] = -1 } } };

        await Assert.That(MarkdownReport.Full(down)).ToContainAsync("- Median latency: http://a/ —");
    }

    [Test("The CPU column says what 100% means")]
    public async Task CpuLabel() =>
        await Assert.That(MarkdownReport.Full(Samples.Summary())).ToContainAsync("| CPU (median, % of one core) |");

    [Test("The full report says how missing records and crashed runs are counted")]
    public async Task MissingRecordsNote() =>
        await Assert.That(MarkdownReport.Full(Samples.Summary())).ToContainAsync(
            "A test that ran but left no timing record counts as a failed run of type `harness`; a run that crashed twice " +
            "with no TRX is counted under run crashes and its tests are missing from that round.");

    [Test("The README block has a pass-rate table, tests by tools, after the summary table")]
    public async Task ReadmePassRates()
    {
        var block = MarkdownReport.ForReadme(Samples.Summary(), "results/x");

        await Assert.That(block).ToContainAsync("| Pass rate per test | Playwright | Selenium |" + Environment.NewLine + "|---|---:|---:|");
        await Assert.That(block).ToContainAsync("| Flakiness.AjaxTests.LabelAppearsAfterAjaxRequest | 100.0% | 98.3% |");
        await Assert.That(block).ToContainAsync("| Flakiness.HiddenLayersTests.GreenButtonCannotBeClickedTwice | 100.0% | 100.0% |");
        await Assert.That(block.IndexOf("| Pass rate, every run |", StringComparison.Ordinal)
            < block.IndexOf("| Pass rate per test | Playwright | Selenium |", StringComparison.Ordinal)).ToBeTrueAsync();
        await Assert.That(block.IndexOf("| Pass rate per test | Playwright | Selenium |", StringComparison.Ordinal)
            < block.IndexOf("<picture>", StringComparison.Ordinal)).ToBeTrueAsync();
    }
}
