using System.Globalization;

namespace Versus.Bench.Reporting;

public static class Format
{
    public const string Missing = "—";

    public static string Plural(int n, string noun) =>
        string.Create(CultureInfo.InvariantCulture, $"{n} {noun}{(n == 1 ? "" : "s")}");

    public static string Ms(double? ms) => ms switch
    {
        null or < 0 => Missing,
        < 1000 => string.Create(CultureInfo.InvariantCulture, $"{ms:0} ms"),
        _ => string.Create(CultureInfo.InvariantCulture, $"{ms / 1000:0.0} s"),
    };

    public static string Date(DateTimeOffset date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string Percent(double ratio) => string.Create(CultureInfo.InvariantCulture, $"{ratio * 100:0.0}%");

    public static string Mb(double? mb) => mb is null ? Missing : string.Create(CultureInfo.InvariantCulture, $"{mb:0} MB");
}
