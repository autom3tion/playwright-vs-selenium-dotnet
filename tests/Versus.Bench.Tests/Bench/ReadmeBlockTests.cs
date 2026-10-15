using Precept;
using Versus.Bench;
using Versus.Bench.Reporting;

namespace Versus.Bench.Tests.Bench;

[TestSuite("ReadmeBlock")]
public class ReadmeBlockTests
{
    private const string Readme = "# Title\n\nintro\n<!-- results:start -->\nold\n<!-- results:end -->\n\n## Methodology\n";

    [Test("Replaces only what is between the markers")]
    public async Task Replaces() =>
        await Assert.That(ReadmeBlock.Replace(Readme, "new"))
            .ToBeEqualAsync("# Title\n\nintro\n<!-- results:start -->\nnew\n<!-- results:end -->\n\n## Methodology\n");

    [Test("Running it twice gives the same file")]
    public async Task Idempotent() =>
        await Assert.That(ReadmeBlock.Replace(ReadmeBlock.Replace(Readme, "new"), "new"))
            .ToBeEqualAsync(ReadmeBlock.Replace(Readme, "new"));

    [Test("A marker named in prose above the block does not confuse the replacement")]
    public async Task ProseMentioningEnd() =>
        await Assert.That(ReadmeBlock.Replace("Mention <!-- results:end --> here.\n<!-- results:start -->\nold\n<!-- results:end -->\ntail\n", "new"))
            .ToBeEqualAsync("Mention <!-- results:end --> here.\n<!-- results:start -->\nnew\n<!-- results:end -->\ntail\n");

    [Test("A README without the markers is an error, not a silent no-op")]
    public async Task MissingMarkers() =>
        await Assert.That(() => { ReadmeBlock.Replace("# Title\n", "new"); return Task.CompletedTask; })
            .ToThrowAsync<BenchException>();
}
