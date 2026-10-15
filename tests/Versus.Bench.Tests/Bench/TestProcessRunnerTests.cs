using Precept;
using Versus.Bench.Running;

namespace Versus.Bench.Tests.Bench;

[TestSuite("TestProcessRunner")]
public class TestProcessRunnerTests
{
    [Test("Runs the built assembly directly, with a TRX file named after the tool")]
    public async Task Arguments()
    {
        var spec = new RunSpec("Selenium", 3, 2, "/repo/Versus.Selenium.Tests.dll", "/raw/w2/r003/Selenium",
            new Dictionary<string, string>(), "class:*.Journeys.*");

        await Assert.That(string.Join(" ", TestProcessRunner.Arguments(spec))).ToBeEqualAsync(
            "/repo/Versus.Selenium.Tests.dll --report-trx --report-trx-filename Selenium.trx " +
            "--results-directory /raw/w2/r003/Selenium --precept-filter class:*.Journeys.*");
    }

    [TestCase(0, true, false)]   // every test passed
    [TestCase(2, true, false)]   // some tests failed: normal
    [TestCase(2, false, true)]   // no TRX file
    [TestCase(134, true, true)]  // killed
    [TestCase(8, true, true)]    // no tests ran
    public async Task Crash(int exitCode, bool trxExists, bool crashed) =>
        await Assert.That(TestProcessRunner.IsCrash(exitCode, trxExists)).ToBeEqualAsync(crashed);

    [Test("A run that cannot start the assembly is retried once, then recorded as a crash")]
    public async Task CrashPath()
    {
        var root = Directory.CreateTempSubdirectory("versus-crash-").FullName;
        var directory = Path.Combine(root, "w1", "r001", "Selenium");
        var spec = new RunSpec("Selenium", 1, 1, Path.Combine(root, "missing.dll"), directory,
            new Dictionary<string, string>(), null);

        var record = await TestProcessRunner.RunAsync(spec);

        await Assert.That(record.Crashed).ToBeTrueAsync();
        await Assert.That(record.Attempts).ToBeEqualAsync(2);
        await Assert.That(File.Exists(Path.Combine(directory, "run.json"))).ToBeTrueAsync();
    }

    [Test("A crashed first attempt is kept beside the run folder, with its output log")]
    public async Task KeepsFirstAttempt()
    {
        var root = Directory.CreateTempSubdirectory("versus-attempt-").FullName;
        var directory = Path.Combine(root, "w1", "r001", "Selenium");
        var spec = new RunSpec("Selenium", 1, 1, Path.Combine(root, "missing.dll"), directory,
            new Dictionary<string, string>(), null);

        await TestProcessRunner.RunAsync(spec);

        var attempt = Path.Combine(root, "w1", "r001", "Selenium.attempt1");
        await Assert.That(File.Exists(Path.Combine(attempt, "output.log"))).ToBeTrueAsync();
        await Assert.That(File.Exists(Path.Combine(directory, "output.log"))).ToBeTrueAsync();
    }

    [Test("A run that hangs is killed after 60 minutes")]
    public async Task Timeout() =>
        await Assert.That(TestProcessRunner.RunTimeout).ToBeEqualAsync(TimeSpan.FromMinutes(60));
}
