using Microsoft.Extensions.DependencyInjection;
using Precept;

namespace Versus.Selenium.Tests;

public sealed class Startup : IPreceptStartup
{
    public void ConfigureServices(IServiceCollection services, PreceptSettings settings) =>
        services.AddPreceptSettings<VersusSettings>();
}
