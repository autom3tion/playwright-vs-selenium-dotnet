using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

/// <summary>
/// Only the cases both tools agree on. Opacity 0, offscreen and overlapped buttons are "visible" to
/// Playwright and partly not to Selenium; the README explains that difference instead.
/// </summary>
[TestSuite("visibility: tell hidden buttons from visible ones")]
public class VisibilityTests : SeleniumTest
{
    [Test]
    public async Task HiddenButtonsAreNotVisible()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/visibility");
        Driver.FindElement(By.Id("hideButton")).Click();

        await Assert.That(Driver.FindElements(By.Id("removedButton"))).ToBeEmptyAsync();
        await Assert.That(Driver.FindElement(By.Id("zeroWidthButton")).Displayed).ToBeFalseAsync();
        await Assert.That(Driver.FindElement(By.Id("invisibleButton")).Displayed).ToBeFalseAsync();
        await Assert.That(Driver.FindElement(By.Id("notdisplayedButton")).Displayed).ToBeFalseAsync();
    }
}
