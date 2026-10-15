using Microsoft.Playwright;
using Versus.Timing;

namespace Versus.Playwright.Tests.Fixtures;

/// <summary>One browser for the whole run, shared by every worker. Disposed by Precept's container after the run.</summary>
public sealed class BrowserHost(VersusSettings settings) : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("The browser has not been launched.");

    public async Task StartAsync()
    {
        var clock = new TestClock(TimeProvider.System);
        clock.Start();

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            ExecutablePath = settings.ChromePath.Length > 0 ? settings.ChromePath : null,
            // No extra Chrome switches. Playwright's own defaults already load the http-only playground on Chrome 155
            // (they include --disable-features=...,HttpsUpgrades,...; measured). Adding our own --disable-features would
            // replace that whole default list, because Chrome keeps only the last copy of a switch. SeleniumTest.cs
            // adds --disable-features=HttpsUpgrades,HttpsFirstBalancedModeAutoEnable, which chromedriver needs.
        });
        clock.SetupDone();

        Assertions.SetDefaultExpectTimeout(settings.TimeoutSeconds * 1000);

        if (RunContext.FromEnvironment() is { } run)
        {
            var (launchMs, _, _) = clock.Finish();
            TimingSink.Write(run, new TimingRecord(Tools.Playwright, TimingRecord.LaunchKey, run.Round, run.Workers,
                launchMs, 0, 0, "Passed", null, null, clock.StartedUtc));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }
}
