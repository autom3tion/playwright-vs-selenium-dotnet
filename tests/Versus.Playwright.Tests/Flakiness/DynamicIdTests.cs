using Microsoft.Playwright;
using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("dynamicid: the button's id changes on every load")]
public class DynamicIdTests : PlaywrightTest
{
    [Test]
    public async Task ButtonIsFoundWithoutItsId()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/dynamicid");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Button with Dynamic ID" }).ClickAsync();
    }
}
