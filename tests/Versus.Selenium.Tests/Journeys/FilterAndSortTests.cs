using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Journeys;

[TestSuite("Filter and sort: hammers, cheapest first")]
public class FilterAndSortTests : SeleniumTest
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
    public void HammersSortedByPriceAscending()
    {
        Driver.Navigate().GoToUrl(Settings.ToolshopUrl);
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        Wait.Until(d => d.FindElements(By.CssSelector("[data-test='product-name']")).Count == 9);

        WaitForClickable(By.XPath("//label[normalize-space()='Hammer']/input")).Click();
        // Toolshop's own requests can arrive out of order; wait for this state as a user of the app would.
        Wait.Until(d => d.FindElements(By.CssSelector("[data-test='filter_completed']")).Count == 1);
        new SelectElement(Driver.FindElement(By.CssSelector("[data-test='sort']"))).SelectByValue("price,asc");

        // The list re-renders twice; wait until the whole list matches, in order.
        WaitForTexts(By.CssSelector("[data-test='product-name']"), HammersByPrice);
    }
}
