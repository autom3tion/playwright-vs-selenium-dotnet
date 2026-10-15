using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("autowait: the button is hidden and disabled for 5 seconds, then restored")]
public class AutoWaitTests : SeleniumTest
{
    [Test]
    public void ButtonIsClickedOnceItBecomesInteractable()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/autowait");

        // Choosing the type first also avoids a page bug that shrinks the target on the first Apply.
        new SelectElement(Driver.FindElement(By.Id("element-type"))).SelectByValue("button");
        Driver.FindElement(By.Id("visible")).Click();
        Driver.FindElement(By.Id("enabled")).Click();
        Driver.FindElement(By.Id("applyButton5")).Click();

        WaitForClickable(By.Id("target")).Click();
        Wait.Until(d => d.FindElement(By.Id("opstatus")).Text == "Target clicked.");
    }
}
