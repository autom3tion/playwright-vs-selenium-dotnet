using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Journeys;

/// <summary>Stops at the payment step. Never click [data-test='finish']: that places an order on a shared public site.</summary>
[TestSuite("Checkout: a signed-in customer reaches the payment step")]
public class CheckoutTests : PlaywrightTest
{
    [Test]
    public async Task ReachesPaymentStepWithoutOrdering()
    {
        await Page.GotoAsync($"{Settings.ToolshopUrl}/auth/login");
        await Page.Locator("[data-test='email']").FillAsync(Settings.ToolshopEmail);
        await Page.Locator("[data-test='password']").FillAsync(Settings.ToolshopPassword);
        await Page.Locator("[data-test='login-submit']").ClickAsync();
        await Expect(Page.Locator("[data-test='page-title']")).ToHaveTextAsync("My account");

        await Page.GotoAsync(Settings.ToolshopUrl);
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveCountAsync(9);
        await Page.Locator("[data-test='search-query']").FillAsync("Thor Hammer");
        await Page.Locator("[data-test='search-submit']").ClickAsync();
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveTextAsync(new[] { "Thor Hammer" });
        await Page.Locator("[data-test='product-name']", new() { HasTextString = "Thor Hammer" }).ClickAsync();
        await Page.Locator("[data-test='add-to-cart']").ClickAsync();
        await Expect(Page.Locator("[data-test='cart-quantity']")).ToHaveTextAsync("1");

        await Page.Locator("[data-test='nav-cart']").ClickAsync();
        await Page.Locator("[data-test='proceed-1']").ClickAsync();
        await Page.Locator("[data-test='proceed-2']").ClickAsync();

        // The demo profile is shared and may have been edited, so fill the address rather than trust it.
        // Country, postcode and house number trigger a lookup that fills street, city and state.
        await Page.Locator("[data-test='country']").SelectOptionAsync("AT");
        await Page.Locator("[data-test='postal_code']").FillAsync("1010");
        await Page.Locator("[data-test='house_number']").FillAsync("1");
        await Expect(Page.Locator("[data-test='street']")).ToHaveValueAsync("Marvin-Krenn-Gasse");
        await Page.Locator("[data-test='proceed-3']").ClickAsync();

        await Expect(Page.Locator("[data-test='payment-method']")).ToBeVisibleAsync();
    }
}
