using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("clientdelay: content appears after 15 seconds of client-side work")]
public class ClientDelayTests : PlaywrightTest
{
    [Test]
    public async Task LabelAppearsAfterClientSideDelay()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/clientdelay");
        await Page.Locator("#ajaxButton").ClickAsync();
        await Expect(Page.Locator("#content p.bg-success")).ToHaveTextAsync("Data calculated on the client side.");
    }
}
