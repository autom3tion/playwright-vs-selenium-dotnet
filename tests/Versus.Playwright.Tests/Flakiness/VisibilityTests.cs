using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

/// <summary>
/// Only the cases both tools agree on. Opacity 0, offscreen and overlapped buttons are "visible" to
/// Playwright and partly not to Selenium; the README explains that difference instead.
/// </summary>
[TestSuite("visibility: tell hidden buttons from visible ones")]
public class VisibilityTests : PlaywrightTest
{
    [Test]
    public async Task HiddenButtonsAreNotVisible()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/visibility");
        await Page.Locator("#hideButton").ClickAsync();

        await Expect(Page.Locator("#removedButton")).ToHaveCountAsync(0);
        await Expect(Page.Locator("#zeroWidthButton")).ToBeHiddenAsync();
        await Expect(Page.Locator("#invisibleButton")).ToBeHiddenAsync();
        await Expect(Page.Locator("#notdisplayedButton")).ToBeHiddenAsync();
    }
}
