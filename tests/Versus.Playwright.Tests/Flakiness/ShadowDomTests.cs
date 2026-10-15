using System.Text.RegularExpressions;
using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

/// <summary>The page's copy-to-clipboard step needs a secure origin, and the site is http only, so it is not tested.</summary>
[TestSuite("shadowdom: work with controls inside an open shadow root")]
public class ShadowDomTests : PlaywrightTest
{
    [Test]
    public async Task GeneratedGuidHasTheRightFormat()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/shadowdom");

        // Playwright's CSS engine pierces open shadow roots.
        await Page.Locator("guid-generator #buttonGenerate").ClickAsync();
        await Expect(Page.Locator("guid-generator #editField"))
            .ToHaveValueAsync(new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"));
    }
}
