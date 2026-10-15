using Precept;
using Versus.Bench.Chrome;
using Versus.Bench.Options;
using Versus.Bench.Running;
using Versus.Timing;

namespace Versus.Bench.Tests.Bench;

[TestSuite("RunEnvironment")]
public class RunEnvironmentTests
{
    private static readonly ChromePaths Chrome = new("/c/chrome", "/c/chromedriver", "155.0.8059.79");

    [Test("Sets parallelism, the shared Chrome, and the timing variables")]
    public async Task SetsVariables()
    {
        var env = RunEnvironment.For(BenchOptions.Parse(["run"]), Chrome, "/raw/w2/r003/Playwright/timing", 3, 2);

        await Assert.That(env["PRECEPT_MAXPARALLELISM"]).ToBeEqualAsync("2");
        await Assert.That(env["PRECEPT_VERSUS__CHROMEPATH"]).ToBeEqualAsync("/c/chrome");
        await Assert.That(env["PRECEPT_VERSUS__CHROMEDRIVERPATH"]).ToBeEqualAsync("/c/chromedriver");
        await Assert.That(env[RunContext.DirectoryVariable]).ToBeEqualAsync("/raw/w2/r003/Playwright/timing");
        await Assert.That(env[RunContext.RoundVariable]).ToBeEqualAsync("3");
        await Assert.That(env[RunContext.WorkersVariable]).ToBeEqualAsync("2");
        await Assert.That(env.ContainsKey("PRECEPT_VERSUS__PLAYGROUNDURL")).ToBeFalseAsync();
    }

    [Test("Passes URL overrides through")]
    public async Task PassesUrls()
    {
        var options = BenchOptions.Parse(["run", "--playground-url", "http://localhost:5000", "--toolshop-url", "http://localhost:4200"]);
        var env = RunEnvironment.For(options, Chrome, "/t", 1, 1);

        await Assert.That(env["PRECEPT_VERSUS__PLAYGROUNDURL"]).ToBeEqualAsync("http://localhost:5000");
        await Assert.That(env["PRECEPT_VERSUS__TOOLSHOPURL"]).ToBeEqualAsync("http://localhost:4200");
    }

    [Test("Raw layout: one folder per worker count, round and tool")]
    public async Task Layout()
    {
        var dir = RawLayout.RunDirectory("/raw", 4, 7, Tools.Selenium);
        await Assert.That(dir).ToBeEqualAsync(Path.Combine("/raw", "w4", "r007", "Selenium"));
        await Assert.That(RawLayout.TimingDirectory(dir)).ToBeEqualAsync(Path.Combine(dir, "timing"));
        await Assert.That(RawLayout.TrxFileName(Tools.Selenium)).ToBeEqualAsync("Selenium.trx");
    }
}
