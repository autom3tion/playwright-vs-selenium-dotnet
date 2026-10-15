using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("autowait: the button is hidden and disabled for 5 seconds, then restored")]
public class AutoWaitTests : PlaywrightTest
{
    [Test]
    public async Task ButtonIsClickedOnceItBecomesInteractable()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/autowait");

        // Choosing the type first also avoids a page bug that shrinks the target on the first Apply.
        await Page.Locator("#element-type").SelectOptionAsync("button");
        await Page.Locator("#visible").UncheckAsync();
        await Page.Locator("#enabled").UncheckAsync();
        await Page.Locator("#applyButton5").ClickAsync();

        await Page.Locator("#target").ClickAsync();
        await Expect(Page.Locator("#opstatus")).ToHaveTextAsync("Target clicked.");
    }
}
