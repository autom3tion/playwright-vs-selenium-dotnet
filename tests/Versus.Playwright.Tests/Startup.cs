using Microsoft.Extensions.DependencyInjection;
using Precept;
using Versus.Playwright.Tests.Fixtures;

namespace Versus.Playwright.Tests;

public sealed class Startup : IPreceptStartup
{
    public void ConfigureServices(IServiceCollection services, PreceptSettings settings)
    {
        services.AddPreceptSettings<VersusSettings>();
        services.AddSingleton<BrowserHost>();
    }

    public Task BeforeRunAsync(IServiceProvider services) =>
        services.GetRequiredService<BrowserHost>().StartAsync();
}
