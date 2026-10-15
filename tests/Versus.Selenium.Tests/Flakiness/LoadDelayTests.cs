using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("loaddelay: the next page takes about 5 seconds to load")]
public class LoadDelayTests : SeleniumTest
{
    [Test]
    public async Task ButtonAppearsAfterSlowPageLoad()
    {
        Driver.Navigate().GoToUrl(Settings.PlaygroundUrl);
        Driver.FindElement(By.LinkText("Load Delay")).Click();
        WaitForClickable(By.XPath("//button[text()='Button Appearing After Delay']")).Click();
        await Assert.That(Driver.Url).ToEndWithAsync("/loaddelay");
    }
}
