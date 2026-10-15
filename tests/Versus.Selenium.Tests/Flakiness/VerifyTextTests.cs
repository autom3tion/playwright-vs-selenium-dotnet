using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("verifytext: the text has extra whitespace and a nested element")]
public class VerifyTextTests : SeleniumTest
{
    [Test]
    public async Task WelcomeTextIgnoresExtraWhitespace()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/verifytext");
        var text = Driver.FindElement(By.CssSelector("div.bg-primary > span.badge-secondary")).Text;
        await Assert.That(text).ToBeEqualAsync("Welcome UserName!");
    }
}
