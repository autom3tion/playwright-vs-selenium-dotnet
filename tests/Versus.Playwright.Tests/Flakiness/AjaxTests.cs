using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Flakiness;

[TestSuite("ajax: content arrives after a 15-second server delay")]
public class AjaxTests : PlaywrightTest
{
    [Test]
    public async Task LabelAppearsAfterAjaxRequest()
    {
        await Page.GotoAsync($"{Settings.PlaygroundUrl}/ajax");
        await Page.Locator("#ajaxButton").ClickAsync();
        await Expect(Page.Locator("#content p.bg-success")).ToHaveTextAsync("Data loaded with AJAX get request.");
    }
}
