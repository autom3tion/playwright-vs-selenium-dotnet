using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Journeys;

[TestSuite("Add to cart: a product goes into the cart")]
public class AddToCartTests : PlaywrightTest
{
    [Test]
    public async Task CartBadgeCountsTheProduct()
    {
        await Page.GotoAsync(Settings.ToolshopUrl);
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveCountAsync(9);
        await Page.Locator("[data-test='search-query']").FillAsync("Thor Hammer");
        await Page.Locator("[data-test='search-submit']").ClickAsync();
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveTextAsync(new[] { "Thor Hammer" });
        await Page.Locator("[data-test='product-name']", new() { HasTextString = "Thor Hammer" }).ClickAsync();

        await Page.Locator("[data-test='add-to-cart']").ClickAsync();

        await Expect(Page.Locator("#toast-container .toast-message")).ToHaveTextAsync("Product added to shopping cart.");
        await Expect(Page.Locator("[data-test='cart-quantity']")).ToHaveTextAsync("1");
    }
}
