using Precept;
using Versus.Bench.Running;
using Versus.Timing;

namespace Versus.Bench.Tests.Bench;

[TestSuite("RoundPlan")]
public class RoundPlanTests
{
    [Test("Round 0 is the warm-up, then the measured rounds")]
    public async Task Rounds() =>
        await Assert.That(string.Join(",", RoundPlan.Rounds(3))).ToBeEqualAsync("0,1,2,3");

    [Test("Odd rounds start with Playwright, even rounds with Selenium")]
    public async Task Alternates()
    {
        await Assert.That(string.Join(",", RoundPlan.Order(1))).ToBeEqualAsync($"{Tools.Playwright},{Tools.Selenium}");
        await Assert.That(string.Join(",", RoundPlan.Order(2))).ToBeEqualAsync($"{Tools.Selenium},{Tools.Playwright}");
    }
}
