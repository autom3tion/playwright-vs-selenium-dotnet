using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Flakiness;

[TestSuite("overlapped: the input is partly under another element and clears itself if typed into there")]
public class OverlappedTests : SeleniumTest
{
    [Test]
    public async Task NameFieldAcceptsTextOnceScrolledIntoView()
    {
        Driver.Navigate().GoToUrl($"{Settings.PlaygroundUrl}/overlapped");
        var name = Driver.FindElement(By.Id("name"));

        // #name is only partly clipped by its 100px scroll container, so scrolling it into view is a no-op
        // and the grey overlay (top: 67px, 50px tall) stays over its centre. Typing there makes the page's
        // input handler clear the field. Scrolling the container to reveal #subject below it moves #name clear.
        new Actions(Driver).ScrollToElement(Driver.FindElement(By.Id("subject"))).Perform();
        name.SendKeys("Ada Lovelace");

        await Assert.That(name.GetDomProperty("value")).ToBeEqualAsync("Ada Lovelace");
    }
}
