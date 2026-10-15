using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("ajax: content arrives after a 15-second server delay")]
public class AjaxTests : SeleniumTest
{
    [Test]
    public async Task LabelAppearsAfterAjaxRequest()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/ajax");
        Driver.FindElement(By.Id("ajaxButton")).Click();
        var label = Wait.Until(d => d.FindElement(By.CssSelector("#content p.bg-success")));
        await Assert.That(label.Text).ToBeEqualAsync("Data loaded with AJAX get request.");
    }
}
