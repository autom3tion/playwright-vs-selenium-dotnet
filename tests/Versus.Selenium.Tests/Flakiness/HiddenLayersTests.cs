using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("hiddenlayers: after one click, another layer covers the green button")]
public class HiddenLayersTests : SeleniumTest
{
    [Test]
    public async Task GreenButtonCannotBeClickedTwice()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/hiddenlayers");
        var green = Driver.FindElement(By.Id("greenButton"));

        green.Click();
        WaitForVisible(By.Id("blueButton"));

        await Assert.That(() => { green.Click(); return Task.CompletedTask; })
            .ToThrowAsync<ElementClickInterceptedException>();
        await Assert.That(Driver.FindElements(By.CssSelector("p.bg-warning"))).ToBeEmptyAsync();
    }
}
