using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

/// <summary>Hovering replaces the link with a clone that counts clicks, so a held reference goes stale.</summary>
[TestSuite("mouseover: the link is re-rendered when the mouse enters it")]
public class MouseOverTests : PlaywrightTest
{
    [Test]
    public async Task ClickCountGoesUpOnEachClick()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/mouseover");

        // The link has no href, so it has no "link" role; its title and class change on hover.
        var link = Page.Locator("a:text-is('Click me')");
        await link.ClickAsync();
        await link.ClickAsync();

        await Expect(Page.Locator("#clickCount")).ToHaveTextAsync("2");
    }
}
