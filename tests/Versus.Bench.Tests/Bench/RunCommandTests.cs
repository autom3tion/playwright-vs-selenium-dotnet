using Precept;
using Versus.Bench;
using Versus.Bench.Options;
using Versus.Bench.Running;

namespace Versus.Bench.Tests.Bench;

[TestSuite("RunCommand")]
public class RunCommandTests
{
    [Test("Refuses to start when the raw folder already holds results, so two benchmarks never mix")]
    public async Task RefusesNonEmptyRaw()
    {
        var root = Directory.CreateTempSubdirectory("versus-run-").FullName;
        var raw = Directory.CreateDirectory(Path.Combine(root, "raw")).FullName;
        File.WriteAllText(Path.Combine(raw, "env.json"), "{}");

        await Assert.That(() => RunCommand.ExecuteAsync(BenchOptions.Parse(["run", "--raw", raw]), new RepoPaths(root)))
            .ToThrowAsync<BenchException>();
    }
}
