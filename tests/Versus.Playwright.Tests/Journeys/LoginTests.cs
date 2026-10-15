using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests.Journeys;

[TestSuite("Log in: the demo customer reaches their account")]
public class LoginTests : PlaywrightTest
{
    [Test]
    public async Task DemoCustomerReachesAccountPage()
    {
        await Page.GotoAsync($"{Settings.ToolshopUrl}/auth/login");
        await Page.Locator("[data-test='email']").FillAsync(Settings.ToolshopEmail);
        await Page.Locator("[data-test='password']").FillAsync(Settings.ToolshopPassword);
        await Page.Locator("[data-test='login-submit']").ClickAsync();

        await Expect(Page.Locator("[data-test='page-title']")).ToHaveTextAsync("My account");
        await Expect(Page.Locator("[data-test='nav-menu']")).ToContainTextAsync("Jane Doe");
    }
}
