using Precept;
using Versus.Timing;

namespace Versus.Bench.Tests.Timing;

[TestSuite("TimingSink")]
public class TimingSinkTests
{
    [Test("Writes a record under <dir>/<tool>/<key>.json and reads it back")]
    public async Task RoundTrips()
    {
        var dir = Directory.CreateTempSubdirectory("versus-timing-").FullName;
        var run = new RunContext(dir, 3, 2);
        var record = new TimingRecord(Tools.Selenium, "Flakiness.AjaxTests.LabelAppears", 3, 2,
            812.5, 15_420.25, 95, "Failed", FailureType.Timeout, "Timed out", DateTimeOffset.Parse("2026-10-20T10:00:00Z"));

        var path = TimingSink.Write(run, record);

        await Assert.That(path).ToBeEqualAsync(Path.Combine(dir, "Selenium", "Flakiness.AjaxTests.LabelAppears.json"));
        await Assert.That(TimingSink.Read(path)).ToBeEqualAsync(record);
    }
}
