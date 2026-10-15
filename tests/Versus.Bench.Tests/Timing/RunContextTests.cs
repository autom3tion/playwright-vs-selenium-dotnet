using Precept;
using Versus.Timing;

namespace Versus.Bench.Tests.Timing;

[TestSuite("RunContext")]
public class RunContextTests
{
    [Test("No timing directory means plain dotnet test: nothing is recorded")]
    public async Task NoDirectory() =>
        await Assert.That(RunContext.From(_ => null)).ToBeNullAsync();

    [Test("Reads directory, round and workers")]
    public async Task ReadsAll()
    {
        var values = new Dictionary<string, string>
        {
            [RunContext.DirectoryVariable] = "/tmp/t",
            [RunContext.RoundVariable] = "7",
            [RunContext.WorkersVariable] = "4",
        };

        var run = RunContext.From(k => values.GetValueOrDefault(k));

        await Assert.That(run).ToBeEqualAsync(new RunContext("/tmp/t", 7, 4));
    }
}
