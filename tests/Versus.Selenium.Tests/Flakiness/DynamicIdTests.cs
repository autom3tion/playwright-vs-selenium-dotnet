using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("dynamicid: the button's id changes on every load")]
public class DynamicIdTests : SeleniumTest
{
    [Test]
    public void ButtonIsFoundWithoutItsId()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/dynamicid");
        Driver.FindElement(By.XPath("//button[text()='Button with Dynamic ID']")).Click();
    }
}
