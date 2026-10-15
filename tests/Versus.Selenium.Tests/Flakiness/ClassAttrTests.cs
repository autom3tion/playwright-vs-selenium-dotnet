using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("classattr: find the primary button by one of several classes")]
public class ClassAttrTests : SeleniumTest
{
    [Test]
    public async Task PrimaryButtonRaisesAlert()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/classattr");
        Driver.FindElement(By.CssSelector("button.btn-primary")).Click();

        // NoAlertPresentException is a NotFoundException, which WebDriverWait ignores while waiting.
        var alert = Wait.Until(d => d.SwitchTo().Alert());
        var text = alert.Text;
        alert.Accept();

        await Assert.That(text).ToBeEqualAsync("Primary button pressed");
    }
}
