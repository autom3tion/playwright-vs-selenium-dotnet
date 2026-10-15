using OpenQA.Selenium;
using Precept;
using Versus.Selenium.Tests.Fixtures;

namespace Versus.Selenium.Tests.Journeys;

[TestSuite("Log in: the demo customer reaches their account")]
public class LoginTests : SeleniumTest
{
    [Test]
    public void DemoCustomerReachesAccountPage()
    {
        Driver.Navigate().GoToUrl($"{Settings.ToolshopUrl}/auth/login");
        WaitForVisible(By.CssSelector("[data-test='email']")).SendKeys(Settings.ToolshopEmail);
        Driver.FindElement(By.CssSelector("[data-test='password']")).SendKeys(Settings.ToolshopPassword);
        Driver.FindElement(By.CssSelector("[data-test='login-submit']")).Click();

        WaitForTexts(By.CssSelector("[data-test='page-title']"), ["My account"]);
        Wait.Until(d => d.FindElement(By.CssSelector("[data-test='nav-menu']")).Text.Contains("Jane Doe"));
    }
}
