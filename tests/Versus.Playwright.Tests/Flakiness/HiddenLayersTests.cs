using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("hiddenlayers: after one click, another layer covers the green button")]
public class HiddenLayersTests : PlaywrightTest
{
    [Test]
    public async Task GreenButtonCannotBeClickedTwice()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/hiddenlayers");
        var green = Page.Locator("#greenButton");

        await green.ClickAsync();
        await Expect(Page.Locator("#blueButton")).ToBeVisibleAsync();

        // A trial click runs every actionability check without clicking. Covered, it times out.
        // This 1-second window is why the test is left out of the speed figures.
        await Assert.That(() => green.ClickAsync(new() { Trial = true, Timeout = 1000 }))
            .ToThrowAsync<TimeoutException>();
        await Expect(Page.Locator("p.bg-warning")).ToHaveCountAsync(0);
    }
}
