using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Journeys;

[TestSuite("Search: a product is found by its name")]
public class SearchTests : PlaywrightTest
{
    [Test]
    public async Task FindsProductByName()
    {
        await Page.GotoAsync(Settings.ToolshopUrl);
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveCountAsync(9);
        await Page.Locator("[data-test='search-query']").FillAsync("Thor Hammer");
        await Page.Locator("[data-test='search-submit']").ClickAsync();

        await Expect(Page.Locator("[data-test='search-caption']")).ToHaveTextAsync("Searched for: Thor Hammer");
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveTextAsync(new[] { "Thor Hammer" });
    }
}
