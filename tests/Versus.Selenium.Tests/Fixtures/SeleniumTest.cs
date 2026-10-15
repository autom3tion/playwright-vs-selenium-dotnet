using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Precept;
using Versus.Timing;

namespace Versus.Selenium.Tests.Fixtures;

/// <summary>A fresh ChromeDriver session per test, the way Selenium's documentation isolates tests.</summary>
public abstract class SeleniumTest
{
    private readonly TestClock _clock = new(TimeProvider.System);

    protected VersusSettings Settings => Services.GetRequiredService<VersusSettings>();

    protected IWebDriver Driver { get; private set; } = null!;

    protected WebDriverWait Wait { get; private set; } = null!;

    private static IServiceProvider Services =>
        PreceptTestContext.Current?.Services ?? throw new InvalidOperationException("No Precept test is running.");

    [BeforeTest]
    public void StartDriver()
    {
        _clock.Start();

        var options = new ChromeOptions();
        if (Settings.ChromePath.Length > 0)
        {
            options.BinaryLocation = Settings.ChromePath;
        }

        // --window-size sets the outer window. Headless Chrome 155 keeps 143 px of browser UI, so 1280x863 gives
        // the same 1280x720 viewport (window.innerWidth x innerHeight) that Playwright sets; measured, see README.
        options.AddArguments("--headless=new", "--window-size=1280,863");

        // Without HttpsFirstBalancedModeAutoEnable, Chrome 155 blocks the http-only playground behind an HTTPS-First
        // interstitial. HttpsUpgrades is also in Playwright's own default --disable-features list. BrowserHost.cs
        // adds no flag: Playwright's defaults already load the playground (measured), and a second
        // --disable-features switch would replace Playwright's whole default list (Chrome keeps the last copy).
        options.AddArgument("--disable-features=HttpsUpgrades,HttpsFirstBalancedModeAutoEnable");

        var service = Settings.ChromeDriverPath.Length > 0
            ? ChromeDriverService.CreateDefaultService(
                Path.GetDirectoryName(Settings.ChromeDriverPath)!, Path.GetFileName(Settings.ChromeDriverPath))
            : ChromeDriverService.CreateDefaultService();
        service.SuppressInitialDiagnosticInformation = true;
        service.HideCommandPromptWindow = true;

        Driver = new ChromeDriver(service, options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(Settings.TimeoutSeconds));
        // The old ExpectedConditions helpers treated a stale element as "not yet"; WaitForVisible/WaitForClickable do too.
        Wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        _clock.SetupDone();
    }

    /// <summary>Waits until the element is displayed, as the old ExpectedConditions.ElementIsVisible did.</summary>
    protected IWebElement WaitForVisible(By by) =>
        Wait.Until(d => d.FindElement(by) is { Displayed: true } e ? e : null)!;

    /// <summary>Waits until the element is displayed and enabled, as ExpectedConditions.ElementToBeClickable did.</summary>
    protected IWebElement WaitForClickable(By by) =>
        Wait.Until(d => d.FindElement(by) is { Displayed: true, Enabled: true } e ? e : null)!;

    /// <summary>
    /// Clicks once nothing covers the element. Mirrors Playwright's hit-target actionability check: WebDriver's click
    /// fails with ElementClickInterceptedException when another element would receive the click, so that exception
    /// means "not yet" here. Used only where an element is known to be covered for a while (the cart toast).
    /// </summary>
    protected void ClickWhenNotCovered(By by)
    {
        var wait = new WebDriverWait(Driver, Wait.Timeout);
        wait.IgnoreExceptionTypes(typeof(ElementClickInterceptedException));
        wait.Until(d =>
        {
            d.FindElement(by).Click();
            return true;
        });
    }

    /// <summary>
    /// Waits until the matching elements' texts are exactly <paramref name="expected"/>, in order. A list
    /// that re-renders while it is read throws StaleElementReferenceException, which Selenium's
    /// documentation says to ignore in such a wait.
    /// </summary>
    protected void WaitForTexts(By by, IReadOnlyList<string> expected)
    {
        var wait = new WebDriverWait(Driver, Wait.Timeout);
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        wait.Until(d => d.FindElements(by).Select(e => e.Text).SequenceEqual(expected));
    }

    [AfterTest]
    public void QuitDriver()
    {
        _clock.BodyDone();
        try
        {
            Driver?.Quit();
        }
        finally
        {
            TestTimings.Write(Tools.Selenium, _clock);
        }
    }
}
