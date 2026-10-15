using Versus.Bench.Running;
using Versus.Timing;

namespace Versus.Bench.Tests.Bench;

internal static class Samples
{
    public static readonly EnvInfo Env = new(
        "test-machine", "macOS 26", "Apple M3", 8, 16L << 30, ".NET 10.0.1", "10.0.400", "155.0.8059.79",
        new Dictionary<string, string> { ["Microsoft.Playwright"] = "1.63.0", ["Selenium.WebDriver"] = "4.51.0" },
        DateTimeOffset.Parse("2026-10-20T09:00:00Z"),
        new Dictionary<string, double> { ["http://uitestingplayground.com/"] = 120 });

    public static TimingRecord Timing(string tool, string key, int workers, bool passed, double bodyMs,
        string? failure = null, double setupMs = 100, int round = 1) =>
        new(tool, key, round, workers, setupMs, bodyMs, 10, passed ? "Passed" : "Failed",
            passed ? null : failure ?? FailureType.Timeout, passed ? null : "failed", DateTimeOffset.UnixEpoch);

    public static RunRecord Run(string tool, int workers, double wallMs, long rssMb, bool crashed = false, int round = 1) =>
        new(tool, round, workers, wallMs, rssMb * 1024 * 1024, wallMs / 1000 * 0.5, crashed ? 134 : 0, crashed, 1);

    public static Versus.Bench.Reporting.Summary Summary(
        double pwSpeed = 10_000, double seSpeed = 18_000, double pwPass = 0.98, double sePass = 0.94,
        double pwMb = 820, double seMb = 560) =>
        new(Env, 20, [1, 2, 4], 1, 0,
            [
                new("Flakiness.AjaxTests.LabelAppearsAfterAjaxRequest", Tools.Playwright, 60, 60, 1, 15_100, 15_600, false, new Dictionary<string, int>()),
                new("Flakiness.AjaxTests.LabelAppearsAfterAjaxRequest", Tools.Selenium, 60, 59, 0.983, 15_400, 16_200, false, new Dictionary<string, int> { ["timeout"] = 1 }),
                new("Flakiness.HiddenLayersTests.GreenButtonCannotBeClickedTwice", Tools.Playwright, 60, 60, 1, null, null, true, new Dictionary<string, int>()),
                new("Flakiness.HiddenLayersTests.GreenButtonCannotBeClickedTwice", Tools.Selenium, 60, 60, 1, null, null, true, new Dictionary<string, int>()),
            ],
            [
                new(Tools.Playwright, 45, 20, 410, pwPass, pwSpeed, 21),
                new(Tools.Selenium, 640, 90, null, sePass, seSpeed, 21),
            ],
            [
                new(Tools.Playwright, 1, 200_000, pwMb, 80), new(Tools.Playwright, 2, 110_000, pwMb * 1.4, 150), new(Tools.Playwright, 4, 70_000, pwMb * 2, 260),
                new(Tools.Selenium, 1, 260_000, seMb, 70), new(Tools.Selenium, 2, 140_000, seMb * 1.9, 140), new(Tools.Selenium, 4, 90_000, seMb * 3.6, 250),
            ],
            [new("Flakiness.AjaxTests.LabelAppearsAfterAjaxRequest", 3, 4), new("(fixture)", 70, 85)]);
}
