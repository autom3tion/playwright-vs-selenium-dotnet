using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("scrolltoclick: page scroll, container scroll, nested scroll, and a button shown only on hover")]
public class ScrollToClickTests : SeleniumTest
{
    [Test]
    public void AllFourButtonsCanBeClicked()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/scrolltoclick");

        // WebDriver's element click scrolls the element into view first.
        Driver.FindElement(By.Id("scrollTarget1")).Click();
        Driver.FindElement(By.Id("scrollTarget2")).Click();
        Driver.FindElement(By.Id("scrollTarget3")).Click();
        new Actions(Driver).MoveToElement(Driver.FindElement(By.Id("targetRow4"))).Perform();
        WaitForClickable(By.Id("scrollTarget4")).Click();

        Wait.Until(d => d.FindElement(By.Id("progressText")).Text == "All buttons clicked!");
    }
}
