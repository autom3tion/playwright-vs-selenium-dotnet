using Precept;
using Versus.Bench.Options;
using Versus.Bench.Running;

namespace Versus.Bench.Tests.Bench;

[TestSuite("SuiteFilter")]
public class SuiteFilterTests
{
    private static readonly Uri Playground = new("http://uitestingplayground.com");
    private static readonly Uri Toolshop = new("https://practicesoftwaretesting.com");

    [Test("All suites: no filter, both sites")]
    public async Task All()
    {
        await Assert.That(SuiteFilter.For(Suite.All)).ToBeNullAsync();
        await Assert.That(SuiteFilter.Sites(Suite.All, Playground, Toolshop)).ToHaveCountAsync(2);
    }

    [Test("Flakiness: Playground only")]
    public async Task Flakiness()
    {
        await Assert.That(SuiteFilter.For(Suite.Flakiness)).ToBeEqualAsync("class:*.Flakiness.*");
        await Assert.That(SuiteFilter.Sites(Suite.Flakiness, Playground, Toolshop).Single()).ToBeEqualAsync(Playground);
    }

    [Test("Journeys: Toolshop only")]
    public async Task Journeys()
    {
        await Assert.That(SuiteFilter.For(Suite.Journeys)).ToBeEqualAsync("class:*.Journeys.*");
        await Assert.That(SuiteFilter.Sites(Suite.Journeys, Playground, Toolshop).Single()).ToBeEqualAsync(Toolshop);
    }
}
