using System.Xml.Linq;
using Precept;
using Versus.Bench.Reporting;

namespace Versus.Bench.Tests.Bench;

[TestSuite("TrxReader")]
public class TrxReaderTests
{
    internal const string Sample = """
        <?xml version="1.0" encoding="utf-8"?>
        <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
          <Results>
            <UnitTestResult testId="a1" testName="LabelAppearsAfterAjaxRequest" outcome="Passed" />
            <UnitTestResult testId="b2" testName="ButtonIsFoundWithoutItsId" outcome="Failed">
              <Output><ErrorInfo><Message>Timed out after 30 seconds</Message></ErrorInfo></Output>
            </UnitTestResult>
          </Results>
          <TestDefinitions>
            <UnitTest name="LabelAppearsAfterAjaxRequest" id="a1">
              <TestMethod className="Versus.Selenium.Tests.Flakiness.AjaxTests" name="LabelAppearsAfterAjaxRequest" />
            </UnitTest>
            <UnitTest name="ButtonIsFoundWithoutItsId" id="b2">
              <TestMethod className="Versus.Selenium.Tests.Flakiness.DynamicIdTests" name="ButtonIsFoundWithoutItsId" />
            </UnitTest>
          </TestDefinitions>
        </TestRun>
        """;

    [Test("Joins each result to its test method")]
    public async Task Parses()
    {
        var results = TrxReader.Parse(XDocument.Parse(Sample));

        await Assert.That(results).ToHaveCountAsync(2);
        await Assert.That(results[0]).ToBeEqualAsync(
            new TrxResult("Versus.Selenium.Tests.Flakiness.AjaxTests", "LabelAppearsAfterAjaxRequest", "Passed", null));
        await Assert.That(results[1].ErrorMessage).ToBeEqualAsync("Timed out after 30 seconds");
    }
}
