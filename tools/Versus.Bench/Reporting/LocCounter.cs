using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Versus.Timing;

namespace Versus.Bench.Reporting;

/// <summary>Non-blank, non-comment lines: of each [Test] method's body, and of the fixture code each tool needs.</summary>
public static class LocCounter
{
    public const string FixtureKey = "(fixture)";

    public static int CountLines(string code) =>
        code.Split('\n').Select(l => l.Trim()).Count(l => l.Length > 0 && !l.StartsWith("//", StringComparison.Ordinal));

    public static IReadOnlyDictionary<string, int> CountTests(string source)
    {
        var counts = new Dictionary<string, int>();
        var root = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();

        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var isTest = method.AttributeLists.SelectMany(l => l.Attributes)
                .Any(a => a.Name.ToString() is "Test" or "TestAttribute");
            if (!isTest)
            {
                continue;
            }

            var type = method.Ancestors().OfType<ClassDeclarationSyntax>().First();
            var ns = method.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().First().Name.ToString();
            var body = (SyntaxNodeText(method.Body) ?? SyntaxNodeText(method.ExpressionBody)) ?? "";
            counts[TestKey.From($"{ns}.{type.Identifier.Text}", method.Identifier.Text)] = CountLines(body);
        }

        return counts;
    }

    public static IReadOnlyList<LocRow> ForRepo(RepoPaths repo)
    {
        var playwright = CountProject(repo.PlaywrightTests);
        var selenium = CountProject(repo.SeleniumTests);

        var rows = playwright.Tests.Keys.Union(selenium.Tests.Keys)
            .Order(StringComparer.Ordinal)
            .Select(key => new LocRow(key, playwright.Tests.GetValueOrDefault(key), selenium.Tests.GetValueOrDefault(key)))
            .ToList();
        rows.Add(new LocRow(FixtureKey, playwright.Fixture, selenium.Fixture));
        return rows;
    }

    private static (IReadOnlyDictionary<string, int> Tests, int Fixture) CountProject(string project)
    {
        var tests = new Dictionary<string, int>();
        var fixture = 0;

        foreach (var file in Directory.EnumerateFiles(project, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(project, file);
            if (relative.StartsWith("bin", StringComparison.Ordinal) || relative.StartsWith("obj", StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(file);
            if (relative.StartsWith("Fixtures", StringComparison.Ordinal) || relative == "Startup.cs")
            {
                fixture += CountLines(source);
                continue;
            }

            foreach (var (key, lines) in CountTests(source))
            {
                tests[key] = lines;
            }
        }

        return (tests, fixture);
    }

    private static string? SyntaxNodeText(Microsoft.CodeAnalysis.SyntaxNode? node) => node?.ToString();
}
