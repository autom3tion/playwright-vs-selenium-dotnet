using System.Runtime.InteropServices;
using Precept;
using Versus.Bench;
using Versus.Bench.Chrome;

namespace Versus.Bench.Tests.Bench;

[TestSuite("ChromeForTesting")]
public class ChromeForTestingTests
{
    [Test("Maps each supported OS and CPU to Chrome for Testing's platform name")]
    public async Task Platforms()
    {
        await Assert.That(ChromeForTesting.Platform(OSPlatform.OSX, Architecture.Arm64)).ToBeEqualAsync("mac-arm64");
        await Assert.That(ChromeForTesting.Platform(OSPlatform.OSX, Architecture.X64)).ToBeEqualAsync("mac-x64");
        await Assert.That(ChromeForTesting.Platform(OSPlatform.Linux, Architecture.X64)).ToBeEqualAsync("linux64");
        await Assert.That(ChromeForTesting.Platform(OSPlatform.Windows, Architecture.X64)).ToBeEqualAsync("win64");
    }

    [Test("Linux on Arm has no Chrome for Testing build")]
    public async Task Unsupported() =>
        await Assert.That(() => { ChromeForTesting.Platform(OSPlatform.Linux, Architecture.Arm64); return Task.CompletedTask; })
            .ToThrowAsync<BenchException>();

    [Test("Download URLs follow the public bucket layout")]
    public async Task Urls() =>
        await Assert.That(ChromeForTesting.DownloadUrl("chromedriver", "mac-arm64").ToString()).ToBeEqualAsync(
            "https://storage.googleapis.com/chrome-for-testing-public/155.0.8059.79/mac-arm64/chromedriver-mac-arm64.zip");

    [Test("Binary paths inside the extracted archives")]
    public async Task Paths()
    {
        await Assert.That(ChromeForTesting.ChromeRelativePath("mac-arm64")).ToBeEqualAsync(
            Path.Combine("chrome-mac-arm64", "Google Chrome for Testing.app", "Contents", "MacOS", "Google Chrome for Testing"));
        await Assert.That(ChromeForTesting.ChromeRelativePath("linux64")).ToBeEqualAsync(Path.Combine("chrome-linux64", "chrome"));
        await Assert.That(ChromeForTesting.ChromeRelativePath("win64")).ToBeEqualAsync(Path.Combine("chrome-win64", "chrome.exe"));
        await Assert.That(ChromeForTesting.DriverRelativePath("linux64")).ToBeEqualAsync(Path.Combine("chromedriver-linux64", "chromedriver"));
        await Assert.That(ChromeForTesting.DriverRelativePath("win64")).ToBeEqualAsync(Path.Combine("chromedriver-win64", "chromedriver.exe"));
    }
}
