using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Journeys;

[TestSuite("Browse: the home page lists a page of products")]
public class BrowseTests : PlaywrightTest
{
    [Test]
    public async Task HomePageShowsProductGrid()
    {
        await Page.GotoAsync(Settings.ToolshopUrl);
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveCountAsync(9);
    }
}
