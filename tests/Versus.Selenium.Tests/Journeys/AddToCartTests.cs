using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Journeys;

[TestSuite("Add to cart: a product goes into the cart")]
public class AddToCartTests : SeleniumTest
{
    [Test]
    public void CartBadgeCountsTheProduct()
    {
        Driver.Navigate().GoToUrl(Settings.ToolshopUrl);
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        Wait.Until(d => d.FindElements(By.CssSelector("[data-test='product-name']")).Count == 9);
        WaitForVisible(By.CssSelector("[data-test='search-query']")).SendKeys("Thor Hammer");
        Driver.FindElement(By.CssSelector("[data-test='search-submit']")).Click();
        WaitForTexts(By.CssSelector("[data-test='product-name']"), ["Thor Hammer"]);
        Driver.FindElement(By.CssSelector("[data-test='product-name']")).Click();

        WaitForClickable(By.CssSelector("[data-test='add-to-cart']")).Click();

        WaitForTexts(By.CssSelector("#toast-container .toast-message"), ["Product added to shopping cart."]);
        WaitForTexts(By.CssSelector("[data-test='cart-quantity']"), ["1"]);
    }
}
