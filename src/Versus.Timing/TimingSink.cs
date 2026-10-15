using System.Text.Json;

namespace Versus.Timing;

public static class TimingSink
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static string Write(RunContext run, TimingRecord record)
    {
        var directory = Path.Combine(run.TimingDirectory, record.Tool);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, record.TestKey + ".json");
        File.WriteAllText(path, JsonSerializer.Serialize(record, Options));
        return path;
    }

    public static TimingRecord Read(string path) =>
        JsonSerializer.Deserialize<TimingRecord>(File.ReadAllText(path), Options)
        ?? throw new InvalidDataException($"'{path}' holds no timing record.");
}
