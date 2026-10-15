using Precept;
using Versus.Bench.Resources;

namespace Versus.Bench.Tests.Bench;

[TestSuite("ProcessTable")]
public class ProcessTableTests
{
    [TestCase("1:02.50", 62.5)]      // macOS ps: m:ss.cc
    [TestCase("01:02:03", 3723.0)]   // Linux ps: hh:mm:ss
    [TestCase("1-01:02:03", 90123.0)] // Linux ps: d-hh:mm:ss
    [TestCase("12.5", 12.5)]         // the PowerShell query on Windows: seconds
    public async Task ParsesCpuTime(string value, double seconds) =>
        await Assert.That(ProcessTable.ParseCpuTime(value).TotalSeconds).ToBeEqualAsync(seconds);

    [Test("Parses 'pid ppid rss-kilobytes cpu' lines and skips anything else")]
    public async Task ParsesLines()
    {
        var rows = ProcessTable.Parse("""
              101     1  20480   0:01.50
              102   101   1024   0:00.25

            garbage line
            """);

        await Assert.That(rows).ToHaveCountAsync(2);
        await Assert.That(rows[1]).ToBeEqualAsync(new ProcessSample(102, 101, 1024 * 1024, TimeSpan.FromSeconds(0.25)));
    }

    [Test("A live snapshot includes this process")]
    public async Task SnapshotIncludesSelf() =>
        await Assert.That((await ProcessTable.SnapshotAsync()).Any(p => p.Pid == Environment.ProcessId)).ToBeTrueAsync();
}
