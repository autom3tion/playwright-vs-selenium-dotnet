using Precept;
using Versus.Bench;
using Versus.Bench.Reporting;

namespace Versus.Bench.Tests.Bench;

[TestSuite("LocCounter")]
public class LocCounterTests
{
    private const string Source = """
        using Precept;

        namespace Versus.Selenium.Tests.Flakiness;

        [TestSuite("ajax")]
        public class AjaxTests : SeleniumTest
        {
            [Test]
            public async Task LabelAppears()
            {
                // a comment
                Driver.Navigate().GoToUrl("x");

                Driver.FindElement(By.Id("ajaxButton")).Click();
            }

            private void Helper() { }
        }
        """;

    [Test("Counts non-blank, non-comment lines")]
    public async Task CountsLines() =>
        await Assert.That(LocCounter.CountLines("a\n\n  // c\n/// d\nb\n")).ToBeEqualAsync(2);

    [Test("Counts each [Test] method's body, braces included, keyed like the timings")]
    public async Task CountsTests()
    {
        var counts = LocCounter.CountTests(Source);

        await Assert.That(counts).ToHaveCountAsync(1);
        await Assert.That(counts["Flakiness.AjaxTests.LabelAppears"]).ToBeEqualAsync(4);
    }

    [Test("Pairs both projects' tests and adds the fixture row")]
    public async Task ForRepo()
    {
        var root = Directory.CreateTempSubdirectory("versus-loc-").FullName;
        foreach (var tool in new[] { "Playwright", "Selenium" })
        {
            var project = Directory.CreateDirectory(Path.Combine(root, "tests", $"Versus.{tool}.Tests", "Flakiness")).Parent!.FullName;
            File.WriteAllText(Path.Combine(project, "Flakiness", "AjaxTests.cs"), Source.Replace("Versus.Selenium.Tests", $"Versus.{tool}.Tests"));
            Directory.CreateDirectory(Path.Combine(project, "Fixtures"));
            File.WriteAllText(Path.Combine(project, "Fixtures", "Base.cs"), tool == "Selenium" ? "a\nb\nc\n" : "a\n");
            File.WriteAllText(Path.Combine(project, "Startup.cs"), "s\n");
        }

        var rows = LocCounter.ForRepo(new RepoPaths(root));

        await Assert.That(rows.Single(r => r.Key == "Flakiness.AjaxTests.LabelAppears")).ToBeEqualAsync(
            new LocRow("Flakiness.AjaxTests.LabelAppears", 4, 4));
        await Assert.That(rows.Single(r => r.Key == LocCounter.FixtureKey)).ToBeEqualAsync(
            new LocRow(LocCounter.FixtureKey, 2, 4));
    }
}
