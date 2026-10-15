using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("clientdelay: content appears after 15 seconds of client-side work")]
public class ClientDelayTests : SeleniumTest
{
    [Test]
    public async Task LabelAppearsAfterClientSideDelay()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/clientdelay");
        Driver.FindElement(By.Id("ajaxButton")).Click();
        var label = Wait.Until(d => d.FindElement(By.CssSelector("#content p.bg-success")));
        await Assert.That(label.Text).ToBeEqualAsync("Data calculated on the client side.");
    }
}
