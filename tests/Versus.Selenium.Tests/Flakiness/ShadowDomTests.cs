using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

/// <summary>The page's copy-to-clipboard step needs a secure origin, and the site is http only, so it is not tested.</summary>
[TestSuite("shadowdom: work with controls inside an open shadow root")]
public class ShadowDomTests : SeleniumTest
{
    [Test]
    public async Task GeneratedGuidHasTheRightFormat()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/shadowdom");

        // Only CSS selectors work inside a shadow root.
        var root = Wait.Until(d => d.FindElement(By.CssSelector("guid-generator")).GetShadowRoot());
        root.FindElement(By.CssSelector("#buttonGenerate")).Click();
        var value = root.FindElement(By.CssSelector("#editField")).GetDomProperty("value") ?? "";

        await Assert.That(value).ToMatchAsync("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");
    }
}
