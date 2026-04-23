using Serilog;
using Serilog.Events;

namespace PTS.Automation.Infrastructure.Reporting;

/// <summary>
/// Serilog wrapper. Configured programmatically from <see cref="TestSettings"/> so
/// we don't need the <c>Serilog.Settings.Configuration</c> package. Keeps dependencies lean.
/// Use <see cref="For{T}"/> to get a logger scoped to a type.
/// </summary>
public static class Log
{
    private static readonly Lazy<ILogger> _root = new(Build);

    public static ILogger Root => _root.Value;

    public static ILogger For<T>() => Root.ForContext<T>();
    public static ILogger For(string name) => Root.ForContext("SourceContext", name);

    private static ILogger Build()
    {
        var s = ConfigFactory.Settings;
        var logDir = Path.Combine(s.Paths.ArtifactsRoot, "logs");
        Directory.CreateDirectory(logDir);

        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logDir, "test-.log"),
                rollingInterval: RollingInterval.Day,
                shared: true,
                outputTemplate:
                    "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Serilog.Log.Logger = logger;
        return logger;
    }

    public static void CloseAndFlush()
    {
        if (_root.IsValueCreated)
            Serilog.Log.CloseAndFlush();
    }
}
