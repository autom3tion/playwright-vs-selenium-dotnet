using Precept;
using Versus.Bench.Reporting;

namespace Versus.Bench.Tests.Bench;

[TestSuite("Stats")]
public class StatsTests
{
    [Test("Median of an even count is the mean of the middle two")]
    public async Task EvenMedian() => await Assert.That(Stats.Median([4, 1, 3, 2])).ToBeEqualAsync(2.5);

    [Test("Median of one value is that value")]
    public async Task SingleMedian() => await Assert.That(Stats.Median([7])).ToBeEqualAsync(7d);

    [Test("p95 interpolates between the closest ranks")]
    public async Task P95() =>
        await Assert.That(Stats.Percentile(Enumerable.Range(1, 20).Select(i => (double)i), 0.95)).ToBeInRangeAsync(19.049, 19.051);

    [Test("No values is an error, not a zero")]
    public async Task Empty() =>
        await Assert.That(() => { Stats.Median([]); return Task.CompletedTask; }).ToThrowAsync<ArgumentException>();

    [TestCase(100.0, 105.0, true)]
    [TestCase(100.0, 106.0, false)]
    [TestCase(106.0, 100.0, false)]
    [TestCase(0.0, 0.0, true)]
    public async Task Tie(double a, double b, bool tie) => await Assert.That(Stats.IsTie(a, b)).ToBeEqualAsync(tie);
}
