using Precept;
using Versus.Bench.Running;

namespace Versus.Bench.Tests.Bench;

[TestSuite("EnvCollector")]
public class EnvCollectorTests
{
    [Test("Reads every pinned package version")]
    public async Task ReadsPackages()
    {
        var packages = EnvCollector.ReadPackages("""
            <Project><ItemGroup>
              <PackageVersion Include="Microsoft.Playwright" Version="1.63.0" />
              <PackageVersion Include="Selenium.WebDriver" Version="4.51.0" />
            </ItemGroup></Project>
            """);

        await Assert.That(packages["Microsoft.Playwright"]).ToBeEqualAsync("1.63.0");
        await Assert.That(packages["Selenium.WebDriver"]).ToBeEqualAsync("4.51.0");
    }

    [TestCase("Sergey's MacBook-Pro.local", "sergey-s-macbook-pro-local")]
    [TestCase("BUILD_AGENT 07", "build-agent-07")]
    public async Task Slug(string name, string slug) =>
        await Assert.That(EnvCollector.MachineSlug(name)).ToBeEqualAsync(slug);
}
