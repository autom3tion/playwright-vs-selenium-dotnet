using System.Globalization;
using Precept;
using Versus.Bench.Reporting;

namespace Versus.Bench.Tests.Bench;

[TestSuite("Format")]
public class FormatTests
{
    [Test("Dates print the same under any culture")]
    public async Task DateIsCultureIndependent()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("th-TH");
            await Assert.That(Format.Date(DateTimeOffset.Parse("2026-10-20T09:00:00Z"))).ToBeEqualAsync("2026-10-20");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
