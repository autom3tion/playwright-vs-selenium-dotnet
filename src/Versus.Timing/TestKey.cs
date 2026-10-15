namespace Versus.Timing;

/// <summary>The name both tools share for one test: the class's namespace without "Versus.&lt;Tool&gt;.Tests", then class and method.</summary>
public static class TestKey
{
    public static string From(string className, string methodName)
    {
        var parts = className.Split('.');
        if (parts.Length < 4 || parts[0] != "Versus" || parts[2] != "Tests")
        {
            throw new ArgumentException($"'{className}' is not in a Versus.<Tool>.Tests.* namespace.", nameof(className));
        }

        return string.Join('.', parts.Skip(3)) + "." + methodName;
    }
}
