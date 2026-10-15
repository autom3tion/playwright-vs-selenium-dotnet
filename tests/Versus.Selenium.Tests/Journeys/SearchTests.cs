using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Journeys;

[TestSuite("Search: a product is found by its name")]
public class SearchTests : SeleniumTest
{
    [Test]
    public void FindsProductByName()
    {
        Driver.Navigate().GoToUrl(Settings.ToolshopUrl);
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        Wait.Until(d => d.FindElements(By.CssSelector("[data-test='product-name']")).Count == 9);
        WaitForVisible(By.CssSelector("[data-test='search-query']")).SendKeys("Thor Hammer");
        Driver.FindElement(By.CssSelector("[data-test='search-submit']")).Click();

        WaitForTexts(By.CssSelector("[data-test='search-caption']"), ["Searched for: Thor Hammer"]);
        WaitForTexts(By.CssSelector("[data-test='product-name']"), ["Thor Hammer"]);
    }
}
