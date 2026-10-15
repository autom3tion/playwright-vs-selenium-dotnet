using Precept;
using Versus.Bench.Reporting;

namespace Versus.Bench.Tests.Bench;

[TestSuite("ChartWriter")]
public class ChartWriterTests
{
    [Test("Writes every chart in a light and a dark version, as SVG")]
    public async Task WritesSvgs()
    {
        var dir = Path.Combine(Directory.CreateTempSubdirectory("versus-charts-").FullName, "charts");

        var files = ChartWriter.WriteAll(Samples.Summary(), dir);

        await Assert.That(files).ToHaveCountAsync(ChartWriter.Charts.Count * 2);
        foreach (var file in files)
        {
            await Assert.That(File.ReadAllText(file)).ToContainAsync("<svg");
        }
    }
}
