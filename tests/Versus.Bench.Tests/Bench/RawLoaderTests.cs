using System.Text.Json;
using Precept;
using Versus.Bench.Reporting;
using Versus.Bench.Running;
using Versus.Timing;

namespace Versus.Bench.Tests.Bench;

[TestSuite("RawLoader")]
public class RawLoaderTests
{
    private static string NewRaw()
    {
        var raw = Directory.CreateTempSubdirectory("versus-raw-").FullName;
        File.WriteAllText(Path.Combine(raw, RawLayout.EnvFile), JsonSerializer.Serialize(Samples.Env, TimingSink.Options));
        return raw;
    }

    private static string AddRun(string raw, RunRecord run)
    {
        var dir = RawLayout.RunDirectory(raw, run.Workers, run.Round, run.Tool);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, RawLayout.RunFile), JsonSerializer.Serialize(run, TimingSink.Options));
        return dir;
    }

    [Test("A test in the TRX file with no timing record counts as a failed 'harness' run")]
    public async Task ReconcilesMissingRecords()
    {
        var raw = NewRaw();
        var dir = AddRun(raw, Samples.Run(Tools.Selenium, 1, 60_000, 500));
        File.WriteAllText(Path.Combine(dir, RawLayout.TrxFileName(Tools.Selenium)), TrxReaderTests.Sample);
        TimingSink.Write(new RunContext(RawLayout.TimingDirectory(dir), 1, 1),
            Samples.Timing(Tools.Selenium, "Flakiness.AjaxTests.LabelAppearsAfterAjaxRequest", 1, true, 15_000));

        var data = RawLoader.Load(raw);

        await Assert.That(data.Timings).ToHaveCountAsync(2);
        var missing = data.Timings.Single(t => t.TestKey == "Flakiness.DynamicIdTests.ButtonIsFoundWithoutItsId");
        await Assert.That(missing.Outcome).ToBeEqualAsync("Failed");
        await Assert.That(missing.FailureType).ToBeEqualAsync(FailureType.Harness);
    }

    [Test("Warm-up rounds are left out")]
    public async Task SkipsWarmUp()
    {
        var raw = NewRaw();
        var dir = AddRun(raw, Samples.Run(Tools.Playwright, 1, 50_000, 700, round: RoundPlan.WarmUpRound));
        TimingSink.Write(new RunContext(RawLayout.TimingDirectory(dir), 0, 1),
            Samples.Timing(Tools.Playwright, "Flakiness.AjaxTests.LabelAppearsAfterAjaxRequest", 1, true, 15_000, round: 0));

        var data = RawLoader.Load(raw);

        await Assert.That(data.Runs).ToBeEmptyAsync();
        await Assert.That(data.Timings).ToBeEmptyAsync();
    }

    [Test("A crashed first attempt kept beside its run folder is not loaded")]
    public async Task SkipsFirstAttempt()
    {
        var raw = NewRaw();
        var dir = AddRun(raw, Samples.Run(Tools.Selenium, 1, 60_000, 500));
        var attempt = RawLayout.FirstAttemptDirectory(dir);
        Directory.CreateDirectory(attempt);
        File.WriteAllText(Path.Combine(attempt, RawLayout.RunFile),
            JsonSerializer.Serialize(Samples.Run(Tools.Selenium, 1, 1_000, 100, crashed: true), TimingSink.Options));

        var data = RawLoader.Load(raw);

        await Assert.That(data.Runs).ToHaveCountAsync(1);
        await Assert.That(data.Runs[0].Crashed).ToBeFalseAsync();
    }

    [Test("No bench.json means no site-down re-runs were logged")]
    public async Task DefaultsLog() =>
        await Assert.That(RawLoader.Load(NewRaw()).Log.SiteDownReruns).ToBeEqualAsync(0);
}
