using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Journeys;

[TestSuite("Browse: the home page lists a page of products")]
public class BrowseTests : SeleniumTest
{
    [Test]
    public void HomePageShowsProductGrid()
    {
        Driver.Navigate().GoToUrl(Settings.ToolshopUrl);
        Wait.Until(d => d.FindElements(By.CssSelector("[data-test='product-name']")).Count == 9);
    }
}
