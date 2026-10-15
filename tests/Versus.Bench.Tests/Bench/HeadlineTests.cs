using Precept;
using Versus.Bench.Reporting;

namespace Versus.Bench.Tests.Bench;

[TestSuite("Headline")]
public class HeadlineTests
{
    [Test("Names a winner for speed, reliability and memory")]
    public async Task Winners() =>
        await Assert.That(Headline.Build(Samples.Summary())).ToBeEqualAsync(
            "Playwright needed 1.8× less total test time across 21 tests, Playwright passed 4.0 pp more often, and Selenium used 32% less memory at 1 worker.");

    [Test("Close results are called ties")]
    public async Task Ties() =>
        await Assert.That(Headline.Build(Samples.Summary(pwSpeed: 10_000, seSpeed: 10_300, pwPass: 0.981, sePass: 0.979, pwMb: 600, seMb: 610)))
            .ToBeEqualAsync("Total test time was a tie, both passed equally often, and memory use was a tie at 1 worker.");

    [Test("One common test reads in the singular")]
    public async Task SingleTest()
    {
        var s = Samples.Summary();
        var one = s with { PerTool = s.PerTool.Select(t => t with { SpeedTests = 1 }).ToList() };

        await Assert.That(Headline.Build(one)).ToContainAsync("1.8× less total test time across 1 test,");
    }
}
