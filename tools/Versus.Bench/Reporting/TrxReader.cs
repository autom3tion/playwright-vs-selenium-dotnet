using System.Xml.Linq;

namespace Versus.Bench.Reporting;

public sealed record TrxResult(string ClassName, string MethodName, string Outcome, string? ErrorMessage);

public static class TrxReader
{
    private static readonly XNamespace Ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    public static IReadOnlyList<TrxResult> Read(string path) => Parse(XDocument.Load(path));

    public static IReadOnlyList<TrxResult> Parse(XDocument trx)
    {
        var methods = trx.Descendants(Ns + "UnitTest")
            .ToDictionary(u => (string)u.Attribute("id")!, u => u.Element(Ns + "TestMethod")!);

        return trx.Descendants(Ns + "UnitTestResult")
            .Select(r =>
            {
                var method = methods[(string)r.Attribute("testId")!];
                return new TrxResult(
                    (string)method.Attribute("className")!,
                    (string)method.Attribute("name")!,
                    (string)r.Attribute("outcome")!,
                    (string?)r.Element(Ns + "Output")?.Element(Ns + "ErrorInfo")?.Element(Ns + "Message"));
            })
            .ToList();
    }
}
