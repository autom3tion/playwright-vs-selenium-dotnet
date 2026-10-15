using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Journeys;

/// <summary>Stops at the payment step. Never click [data-test='finish']: that places an order on a shared public site.</summary>
[TestSuite("Checkout: a signed-in customer reaches the payment step")]
public class CheckoutTests : SeleniumTest
{
    [Test]
    public void ReachesPaymentStepWithoutOrdering()
    {
        Driver.Navigate().GoToUrl($"{Settings.ToolshopUrl}/auth/login");
        WaitForVisible(By.CssSelector("[data-test='email']")).SendKeys(Settings.ToolshopEmail);
        Driver.FindElement(By.CssSelector("[data-test='password']")).SendKeys(Settings.ToolshopPassword);
        Driver.FindElement(By.CssSelector("[data-test='login-submit']")).Click();
        WaitForTexts(By.CssSelector("[data-test='page-title']"), ["My account"]);

        Driver.Navigate().GoToUrl(Settings.ToolshopUrl);
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        Wait.Until(d => d.FindElements(By.CssSelector("[data-test='product-name']")).Count == 9);
        WaitForVisible(By.CssSelector("[data-test='search-query']")).SendKeys("Thor Hammer");
        Driver.FindElement(By.CssSelector("[data-test='search-submit']")).Click();
        WaitForTexts(By.CssSelector("[data-test='product-name']"), ["Thor Hammer"]);
        Driver.FindElement(By.CssSelector("[data-test='product-name']")).Click();
        WaitForClickable(By.CssSelector("[data-test='add-to-cart']")).Click();
        WaitForTexts(By.CssSelector("[data-test='cart-quantity']"), ["1"]);

        // The "added to cart" toast covers the cart link's centre for a few seconds (measured in both tools).
        // Playwright's click waits for the hit target by design; this is WebDriver's closest equivalent.
        ClickWhenNotCovered(By.CssSelector("[data-test='nav-cart']"));
        WaitForClickable(By.CssSelector("[data-test='proceed-1']")).Click();
        WaitForClickable(By.CssSelector("[data-test='proceed-2']")).Click();

        // The demo profile is shared and may have been edited, so fill the address rather than trust it.
        // Country, postcode and house number trigger a lookup that fills street, city and state.
        new SelectElement(WaitForVisible(By.CssSelector("[data-test='country']"))).SelectByValue("AT");
        var postalCode = Driver.FindElement(By.CssSelector("[data-test='postal_code']"));
        postalCode.Clear();
        postalCode.SendKeys("1010");
        var houseNumber = Driver.FindElement(By.CssSelector("[data-test='house_number']"));
        houseNumber.Clear();
        houseNumber.SendKeys("1");
        Wait.Until(d => d.FindElement(By.CssSelector("[data-test='street']")).GetDomProperty("value") == "Marvin-Krenn-Gasse");
        WaitForClickable(By.CssSelector("[data-test='proceed-3']")).Click();

        WaitForVisible(By.CssSelector("[data-test='payment-method']"));
    }
}
