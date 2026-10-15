using Precept;
using Versus.Bench;

namespace Versus.Bench.Tests.Bench;

[TestSuite("RepoPaths")]
public class RepoPathsTests
{
    [Test("Finds the repository root by Versus.slnx, from a folder inside it")]
    public async Task FindsRoot()
    {
        var root = Directory.CreateTempSubdirectory("versus-repo-").FullName;
        File.WriteAllText(Path.Combine(root, "Versus.slnx"), "<Solution />");
        var inner = Directory.CreateDirectory(Path.Combine(root, "tools", "Versus.Bench")).FullName;

        var repo = RepoPaths.Find(inner);

        await Assert.That(repo.Root).ToBeEqualAsync(root);
        await Assert.That(repo.TestAssembly("Selenium")).ToBeEqualAsync(
            Path.Combine(root, "tests", "Versus.Selenium.Tests", "bin", "Release", "net10.0", "Versus.Selenium.Tests.dll"));
    }

    [Test("Outside the repository it says so")]
    public async Task NotFound() =>
        await Assert.That(() => { RepoPaths.Find(Directory.CreateTempSubdirectory("versus-none-").FullName); return Task.CompletedTask; })
            .ToThrowAsync<BenchException>();
}
