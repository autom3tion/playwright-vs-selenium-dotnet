using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

/// <summary>Hovering replaces the link with a clone that counts clicks, so a held reference goes stale.</summary>
[TestSuite("mouseover: the link is re-rendered when the mouse enters it")]
public class MouseOverTests : SeleniumTest
{
    [Test]
    public void ClickCountGoesUpOnEachClick()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/mouseover");

        // Find the link again before each click: the one found before a hover has been replaced.
        Driver.FindElement(By.XPath("//a[text()='Click me']")).Click();
        Driver.FindElement(By.XPath("//a[text()='Click me']")).Click();

        Wait.Until(d => d.FindElement(By.Id("clickCount")).Text == "2");
    }
}
