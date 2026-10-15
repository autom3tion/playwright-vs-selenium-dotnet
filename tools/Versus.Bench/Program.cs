using System.Globalization;
using Versus.Bench;
using Versus.Bench.Options;
using Versus.Bench.Reporting;
using Versus.Bench.Running;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

try
{
    var options = BenchOptions.Parse(args);
    var repo = RepoPaths.Find(Directory.GetCurrentDirectory());
    return options.Command == BenchCommand.Run
        ? await RunCommand.ExecuteAsync(options, repo)
        : ReportCommand.Execute(options, repo);
}
catch (BenchException e)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}
