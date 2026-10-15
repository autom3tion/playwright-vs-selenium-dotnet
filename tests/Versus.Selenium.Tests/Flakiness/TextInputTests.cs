using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("textinput: typing must fire real input events")]
public class TextInputTests : SeleniumTest
{
    [Test]
    public async Task ButtonTakesTheTypedName()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/textinput");
        Driver.FindElement(By.Id("newButtonName")).SendKeys("Versus");
        Driver.FindElement(By.Id("updatingButton")).Click();
        await Assert.That(Driver.FindElement(By.Id("updatingButton")).Text).ToBeEqualAsync("Versus");
    }
}
