using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("textinput: typing must fire real input events")]
public class TextInputTests : PlaywrightTest
{
    [Test]
    public async Task ButtonTakesTheTypedName()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/textinput");
        await Page.Locator("#newButtonName").FillAsync("Versus");
        await Page.Locator("#updatingButton").ClickAsync();
        await Expect(Page.Locator("#updatingButton")).ToHaveTextAsync("Versus");
    }
}
