using Microsoft.Playwright;
using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Journeys;

[TestSuite("Filter and sort: hammers, cheapest first")]
public class FilterAndSortTests : PlaywrightTest
{
    private static readonly string[] HammersByPrice =
    [
        "Thor Hammer",
        "Claw Hammer",
        "Hammer",
        "Claw Hammer with Shock Reduction Grip",
        "Sledgehammer",
        "Court Hammer",
        "Claw Hammer with Fiberglass Handle",
    ];

    [Test]
    public async Task HammersSortedByPriceAscending()
    {
        await Page.GotoAsync(Settings.ToolshopUrl);
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveCountAsync(9);

        // Exact: "Saw" would otherwise also match "Hand Saw", and "Hammer" more than one label.
        await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Hammer", Exact = true }).CheckAsync();
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        await Expect(Page.Locator("[data-test='filter_completed']")).ToHaveCountAsync(1);
        await Page.Locator("[data-test='sort']").SelectOptionAsync("price,asc");

        // The list re-renders twice; the assertion retries until the whole list matches, in order.
        await Expect(Page.Locator("[data-test='product-name']")).ToHaveTextAsync(HammersByPrice);
    }
}
