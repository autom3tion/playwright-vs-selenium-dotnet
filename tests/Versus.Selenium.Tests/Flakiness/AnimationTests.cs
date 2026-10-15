using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("animation: click the target only after it stops moving")]
public class AnimationTests : SeleniumTest
{
    [Test]
    public void MovingTargetIsClickedAfterAnimation()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/animation");
        Driver.FindElement(By.Id("animationButton")).Click();

        // While it moves, the target's class includes "spin".
        Wait.Until(d => !(d.FindElement(By.Id("movingTarget")).GetDomAttribute("class") ?? "").Contains("spin"));
        Driver.FindElement(By.Id("movingTarget")).Click();

        Wait.Until(d => d.FindElement(By.Id("opstatus")).Text
            == "Moving Target clicked. It's class name is 'btn btn-primary'");
    }
}
