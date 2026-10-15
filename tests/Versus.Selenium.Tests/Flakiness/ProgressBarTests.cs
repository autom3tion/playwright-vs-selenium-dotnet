using System.Globalization;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("progressbar: stop the bar as close to 75% as possible")]
public class ProgressBarTests : SeleniumTest
{
    [Test]
    public async Task StopsCloseToSeventyFivePercent()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/progressbar");
        Driver.FindElement(By.Id("startButton")).Click();

        var bar = Driver.FindElement(By.Id("progressBar"));
        Wait.Until(_ => int.Parse(bar.GetDomAttribute("aria-valuenow") ?? "0", CultureInfo.InvariantCulture) >= 75);
        Driver.FindElement(By.Id("stopButton")).Click();

        // "Result: <value - 75>, duration: <ms>"
        var result = Driver.FindElement(By.Id("result")).Text;
        var overshoot = int.Parse(Regex.Match(result, @"Result: (-?\d+)").Groups[1].Value, CultureInfo.InvariantCulture);
        await Assert.That(overshoot).ToBeInRangeAsync(0, 10);
    }
}
