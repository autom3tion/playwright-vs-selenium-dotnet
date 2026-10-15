using Precept;
using Versus.Timing;

namespace Versus.Bench.Tests.Timing;

[TestSuite("TestKey")]
public class TestKeyTests
{
    [TestCase("Versus.Playwright.Tests.Flakiness.AjaxTests", "LabelAppears", "Flakiness.AjaxTests.LabelAppears")]
    [TestCase("Versus.Selenium.Tests.Flakiness.AjaxTests", "LabelAppears", "Flakiness.AjaxTests.LabelAppears")]
    [TestCase("Versus.Selenium.Tests.Journeys.SearchTests", "FindsProduct", "Journeys.SearchTests.FindsProduct")]
    public async Task StripsToolNamespace(string className, string method, string expected) =>
        await Assert.That(TestKey.From(className, method)).ToBeEqualAsync(expected);

    [TestCase("Other.AjaxTests")]
    [TestCase("Versus.Selenium.Flakiness.AjaxTests")]
    public async Task RejectsClassOutsideATestProject(string className) =>
        await Assert.That(() => { TestKey.From(className, "X"); return Task.CompletedTask; })
            .ToThrowAsync<ArgumentException>();
}
