using Precept;
using Versus.Bench.Resources;

namespace Versus.Bench.Tests.Bench;

[TestSuite("ProcessTree")]
public class ProcessTreeTests
{
    [Test("Takes the root and every descendant, nothing else")]
    public async Task Descendants()
    {
        ProcessSample P(int pid, int parent) => new(pid, parent, 0, TimeSpan.Zero);
        var all = new[] { P(1, 0), P(10, 1), P(11, 10), P(12, 11), P(20, 1), P(13, 10) };

        var tree = ProcessTree.Of(all, 10);

        await Assert.That(string.Join(",", tree.Select(p => p.Pid).Order())).ToBeEqualAsync("10,11,12,13");
    }

    [Test("A parent cycle terminates and lists each process once")]
    public async Task Cycle()
    {
        ProcessSample P(int pid, int parent) => new(pid, parent, 0, TimeSpan.Zero);
        var all = new[] { P(10, 11), P(11, 10) };

        var tree = ProcessTree.Of(all, 10);

        await Assert.That(string.Join(",", tree.Select(p => p.Pid).Order())).ToBeEqualAsync("10,11");
    }
}
