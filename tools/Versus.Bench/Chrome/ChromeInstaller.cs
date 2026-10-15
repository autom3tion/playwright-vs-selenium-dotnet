using System.IO.Compression;

namespace Versus.Bench.Chrome;

public sealed class ChromeInstaller(HttpClient http)
{
    public async Task<ChromePaths> EnsureAsync(string cacheRoot)
    {
        var platform = ChromeForTesting.CurrentPlatform();
        var root = Path.Combine(cacheRoot, ChromeForTesting.Version);
        var chrome = Path.Combine(root, ChromeForTesting.ChromeRelativePath(platform));
        var driver = Path.Combine(root, ChromeForTesting.DriverRelativePath(platform));

        if (!File.Exists(chrome))
        {
            await DownloadAndExtractAsync(ChromeForTesting.DownloadUrl("chrome", platform), root);
        }

        if (!File.Exists(driver))
        {
            await DownloadAndExtractAsync(ChromeForTesting.DownloadUrl("chromedriver", platform), root);
        }

        foreach (var binary in new[] { chrome, driver })
        {
            if (!File.Exists(binary))
            {
                throw new BenchException($"'{binary}' is missing after extracting Chrome for Testing {ChromeForTesting.Version}.");
            }
        }

        return new ChromePaths(chrome, driver, ChromeForTesting.Version);
    }

    private async Task DownloadAndExtractAsync(Uri url, string root)
    {
        Directory.CreateDirectory(root);
        var zip = Path.Combine(root, Path.GetFileName(url.AbsolutePath));
        Console.WriteLine($"Downloading {url}");

        try
        {
            await using (var source = await http.GetStreamAsync(url))
            await using (var target = File.Create(zip))
            {
                await source.CopyToAsync(target);
            }
        }
        catch (HttpRequestException e)
        {
            throw new BenchException($"Could not download {url}: {e.Message}");
        }

        if (OperatingSystem.IsWindows())
        {
            ZipFile.ExtractToDirectory(zip, root, overwriteFiles: true);
        }
        else
        {
            // unzip exits 1 for warnings only; a real failure is caught by the binary check in EnsureAsync.
            var result = await Shell.RunAsync("unzip", ["-q", "-o", zip, "-d", root]);
            if (result.ExitCode is not (0 or 1))
            {
                throw new BenchException($"unzip failed for {url}: {result.StdErr}");
            }
        }

        File.Delete(zip);
    }
}
