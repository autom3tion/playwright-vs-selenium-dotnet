using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("scrolltoclick: page scroll, container scroll, nested scroll, and a button shown only on hover")]
public class ScrollToClickTests : PlaywrightTest
{
    [Test]
    public async Task AllFourButtonsCanBeClicked()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/scrolltoclick");

        await Page.Locator("#scrollTarget1").ClickAsync();
        await Page.Locator("#scrollTarget2").ClickAsync();
        await Page.Locator("#scrollTarget3").ClickAsync();
        await Page.Locator("#targetRow4").HoverAsync();
        await Page.Locator("#scrollTarget4").ClickAsync();

        await Expect(Page.Locator("#progressText")).ToHaveTextAsync("All buttons clicked!");
    }
}
