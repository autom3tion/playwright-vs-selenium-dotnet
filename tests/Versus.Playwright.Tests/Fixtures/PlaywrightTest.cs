using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Precept;
using Versus.Timing;

namespace Versus.Playwright.Tests.Fixtures;

/// <summary>A fresh browser context and page per test, the way Playwright's documentation isolates tests.</summary>
public abstract class PlaywrightTest
{
    private readonly TestClock _clock = new(TimeProvider.System);
    private IBrowserContext? _context;

    protected VersusSettings Settings => Services.GetRequiredService<VersusSettings>();

    protected IPage Page { get; private set; } = null!;

    private static IServiceProvider Services =>
        PreceptTestContext.Current?.Services ?? throw new InvalidOperationException("No Precept test is running.");

    protected static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);

    protected static IPageAssertions Expect(IPage page) => Assertions.Expect(page);

    [BeforeTest]
    public async Task OpenContextAsync()
    {
        _clock.Start();
        var browser = Services.GetRequiredService<BrowserHost>().Browser;
        _context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
        });
        _context.SetDefaultTimeout(Settings.TimeoutSeconds * 1000);
        _context.SetDefaultNavigationTimeout(30_000);
        Page = await _context.NewPageAsync();
        _clock.SetupDone();
    }

    [AfterTest]
    public async Task CloseContextAsync()
    {
        _clock.BodyDone();
        try
        {
            if (_context is not null)
            {
                await _context.CloseAsync();
            }
        }
        finally
        {
            TestTimings.Write(Tools.Playwright, _clock);
        }
    }
}
