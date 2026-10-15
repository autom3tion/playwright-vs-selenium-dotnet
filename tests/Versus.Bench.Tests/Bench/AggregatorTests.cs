using Precept;
using Versus.Bench.Reporting;
using Versus.Bench.Running;
using Versus.Timing;
using static Versus.Bench.Tests.Bench.Samples;

namespace Versus.Bench.Tests.Bench;

[TestSuite("Aggregator")]
public class AggregatorTests
{
    private const string Ajax = "Flakiness.AjaxTests.LabelAppearsAfterAjaxRequest";
    private const string Login = "Journeys.LoginTests.DemoCustomerReachesAccountPage";
    private const string Hidden = "Flakiness.HiddenLayersTests.GreenButtonCannotBeClickedTwice";

    private static Summary Summarize(IEnumerable<TimingRecord> timings, IEnumerable<RunRecord>? runs = null) =>
        Aggregator.Summarize(new RawData(Env, new BenchLog(2), (runs ?? []).ToList(), timings.ToList()), []);

    [Test("Pass rate counts every run at every worker count")]
    public async Task PassRateUsesAllRuns()
    {
        var s = Summarize([
            Samples.Timing(Tools.Selenium, Ajax, 1, true, 15_000),
            Samples.Timing(Tools.Selenium, Ajax, 4, false, 30_000),
            Samples.Timing(Tools.Selenium, Ajax, 4, true, 15_500),
            Samples.Timing(Tools.Selenium, Ajax, 2, true, 15_200),
        ]);

        var ajax = s.PerTest.Single(t => t.Tool == Tools.Selenium);
        await Assert.That(ajax.Runs).ToBeEqualAsync(4);
        await Assert.That(ajax.PassRate).ToBeEqualAsync(0.75);
        await Assert.That(ajax.Failures[FailureType.Timeout]).ToBeEqualAsync(1);
    }

    [Test("Speed uses only passed runs at 1 worker")]
    public async Task SpeedUsesPassedSingleWorker()
    {
        var s = Summarize([
            Samples.Timing(Tools.Playwright, Ajax, 1, true, 15_000),
            Samples.Timing(Tools.Playwright, Ajax, 1, true, 16_000),
            Samples.Timing(Tools.Playwright, Ajax, 1, false, 30_000),
            Samples.Timing(Tools.Playwright, Ajax, 4, true, 25_000),
        ]);

        await Assert.That(s.PerTest.Single().MedianBodyMs).ToBeEqualAsync(15_500d);
    }

    [Test("A test that never passed has no speed figure and a 0% pass rate")]
    public async Task NeverPassed()
    {
        var s = Summarize([
            Samples.Timing(Tools.Selenium, Login, 1, false, 30_000),
            Samples.Timing(Tools.Selenium, Login, 1, false, 30_000),
        ]);

        var login = s.PerTest.Single();
        await Assert.That(login.MedianBodyMs).ToBeNullAsync();
        await Assert.That(login.P95BodyMs).ToBeNullAsync();
        await Assert.That(login.PassRate).ToBeEqualAsync(0d);
    }

    [Test("hiddenlayers is left out of speed but counts for pass rate")]
    public async Task HiddenLayersExcluded()
    {
        var s = Summarize([Samples.Timing(Tools.Playwright, Hidden, 1, true, 1_400)]);

        var hidden = s.PerTest.Single();
        await Assert.That(hidden.SpeedExcluded).ToBeTrueAsync();
        await Assert.That(hidden.MedianBodyMs).ToBeNullAsync();
        await Assert.That(hidden.PassRate).ToBeEqualAsync(1d);
    }

    [Test("The speed total only adds tests both tools have a figure for")]
    public async Task SpeedTotalOnCommonTests()
    {
        var s = Summarize([
            Samples.Timing(Tools.Playwright, Ajax, 1, true, 15_000),
            Samples.Timing(Tools.Selenium, Ajax, 1, true, 15_600),
            Samples.Timing(Tools.Playwright, Login, 1, true, 2_000),
            Samples.Timing(Tools.Selenium, Login, 1, false, 30_000),
        ]);

        var pw = s.PerTool.Single(t => t.Tool == Tools.Playwright);
        await Assert.That(pw.SpeedTotalMs).ToBeEqualAsync(15_000d);
        await Assert.That(pw.SpeedTests).ToBeEqualAsync(1);
    }

    [Test("Setup median ignores harness records, which have no real timings")]
    public async Task SetupIgnoresHarness()
    {
        var s = Summarize([
            Samples.Timing(Tools.Selenium, Ajax, 1, true, 15_000, setupMs: 600),
            Samples.Timing(Tools.Selenium, Login, 1, false, 0, FailureType.Harness, setupMs: 0),
        ]);

        await Assert.That(s.PerTool.Single(t => t.Tool == Tools.Selenium).MedianSetupMs).ToBeEqualAsync(600d);
    }

    [Test("Launch time comes from the _launch record, which is not a test")]
    public async Task Launch()
    {
        var s = Summarize([
            Samples.Timing(Tools.Playwright, TimingRecord.LaunchKey, 1, true, 0, setupMs: 400),
            Samples.Timing(Tools.Playwright, Ajax, 1, true, 15_000),
        ]);

        await Assert.That(s.PerTool.Single(t => t.Tool == Tools.Playwright).MedianLaunchMs).ToBeEqualAsync(400d);
        await Assert.That(s.PerTest).ToHaveCountAsync(1);
    }

    [Test("Scaling uses medians per worker count and skips crashed runs")]
    public async Task Scaling()
    {
        var s = Summarize([], [
            Run(Tools.Selenium, 2, 100_000, 600),
            Run(Tools.Selenium, 2, 120_000, 700),
            Run(Tools.Selenium, 2, 999_000, 9_000, crashed: true),
        ]);

        var point = s.Scaling.Single();
        await Assert.That(point.MedianWallMs).ToBeEqualAsync(110_000d);
        await Assert.That(point.MedianPeakRssMb).ToBeEqualAsync(650d);
        await Assert.That(s.RunCrashes).ToBeEqualAsync(1);
        await Assert.That(s.SiteDownReruns).ToBeEqualAsync(2);
    }
}
