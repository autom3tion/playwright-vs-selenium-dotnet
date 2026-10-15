using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("loaddelay: the next page takes about 5 seconds to load")]
public class LoadDelayTests : PlaywrightTest
{
    [Test]
    public async Task ButtonAppearsAfterSlowPageLoad()
    {
        await Page.GotoAsync(Settings.PlaygroundUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Load Delay" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Button Appearing After Delay" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/loaddelay$"));
    }
}
