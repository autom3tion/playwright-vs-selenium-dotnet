using System.Globalization;
using System.Text.RegularExpressions;
using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("progressbar: stop the bar as close to 75% as possible")]
public class ProgressBarTests : PlaywrightTest
{
    [Test]
    public async Task StopsCloseToSeventyFivePercent()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/progressbar");
        await Page.Locator("#startButton").ClickAsync();

        // 75 or more: the bar can pass 75 between two polls.
        await Expect(Page.Locator("#progressBar")).ToHaveAttributeAsync("aria-valuenow", new Regex("^(7[5-9]|[89][0-9]|100)$"));
        await Page.Locator("#stopButton").ClickAsync();

        // "Result: <value - 75>, duration: <ms>"
        var result = await Page.Locator("#result").TextContentAsync() ?? "";
        var overshoot = int.Parse(Regex.Match(result, @"Result: (-?\d+)").Groups[1].Value, CultureInfo.InvariantCulture);
        await Assert.That(overshoot).ToBeInRangeAsync(0, 10);
    }
}
