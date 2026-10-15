using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("verifytext: the text has extra whitespace and a nested element")]
public class VerifyTextTests : PlaywrightTest
{
    [Test]
    public async Task WelcomeTextIgnoresExtraWhitespace()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/verifytext");
        await Expect(Page.Locator("div.bg-primary > span.badge-secondary")).ToHaveTextAsync("Welcome UserName!");
    }
}
