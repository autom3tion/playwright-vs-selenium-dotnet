using Precept;
using Versus.Bench;
using Versus.Bench.Options;

namespace Versus.Bench.Tests.Bench;

[TestSuite("BenchOptions")]
public class BenchOptionsTests
{
    [Test("run with no options uses the polite defaults")]
    public async Task Defaults()
    {
        var o = BenchOptions.Parse(["run"]);

        await Assert.That(o.Command).ToBeEqualAsync(BenchCommand.Run);
        await Assert.That(o.Rounds).ToBeEqualAsync(20);
        await Assert.That(string.Join(",", o.Workers)).ToBeEqualAsync("1,2,4");
        await Assert.That(o.Suite).ToBeEqualAsync(Suite.All);
        await Assert.That(o.AllowHighLoad).ToBeFalseAsync();
    }

    [Test("Reads every option")]
    public async Task ReadsOptions()
    {
        var o = BenchOptions.Parse(["run", "--rounds", "3", "--workers", "4,1", "--suite", "journeys",
            "--playground-url", "http://localhost:5000", "--toolshop-url", "http://localhost:4200"]);

        await Assert.That(o.Rounds).ToBeEqualAsync(3);
        await Assert.That(string.Join(",", o.Workers)).ToBeEqualAsync("1,4");
        await Assert.That(o.Suite).ToBeEqualAsync(Suite.Journeys);
        await Assert.That(o.PlaygroundUrl).ToBeEqualAsync("http://localhost:5000");
        await Assert.That(o.ToolshopUrl).ToBeEqualAsync("http://localhost:4200");
    }

    [Test("More than 4 workers against the hosted sites is refused")]
    public async Task RefusesHighLoad() =>
        await Assert.That(() => { BenchOptions.Parse(["run", "--workers", "8"]); return Task.CompletedTask; })
            .ToThrowAsync<BenchException>();

    [Test("--allow-high-load lifts the limit")]
    public async Task AllowsHighLoad() =>
        await Assert.That(string.Join(",", BenchOptions.Parse(["run", "--workers", "8", "--allow-high-load"]).Workers))
            .ToBeEqualAsync("8");

    [TestCase("")]
    [TestCase("bench")]
    public async Task RejectsBadCommand(string command) =>
        await Assert.That(() => { BenchOptions.Parse(command.Length == 0 ? [] : [command]); return Task.CompletedTask; })
            .ToThrowAsync<BenchException>();

    [TestCase("--rounds", "0")]
    [TestCase("--rounds", "x")]
    [TestCase("--workers", "0")]
    [TestCase("--suite", "smoke")]
    [TestCase("--colour", "red")]
    public async Task RejectsBadOption(string name, string value) =>
        await Assert.That(() => { BenchOptions.Parse(["run", name, value]); return Task.CompletedTask; })
            .ToThrowAsync<BenchException>();

    [Test("An option with no value is refused")]
    public async Task RejectsMissingValue() =>
        await Assert.That(() => { BenchOptions.Parse(["run", "--rounds"]); return Task.CompletedTask; })
            .ToThrowAsync<BenchException>();
}
