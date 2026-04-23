using Microsoft.Extensions.Configuration;

namespace PTS.Automation.Infrastructure.Config;

/// <summary>
/// Single source of truth for building <see cref="TestSettings"/>.
/// Precedence (later wins):
///   1. appsettings.json                   (shared defaults)
///   2. appsettings.{TEST_ENV}.json        (e.g. appsettings.qa.json)
///   3. appsettings.local.json             (gitignored developer overrides)
///   4. User-secrets                       (local dev credentials)
///   5. Environment variables "PTS_*"      (CI secrets; __ separator for nested keys)
/// </summary>
public static class ConfigFactory
{
    private static readonly Lazy<TestSettings> _cached = new(Build);
    public static TestSettings Settings => _cached.Value;

    public static string CurrentEnvironment =>
        System.Environment.GetEnvironmentVariable("TEST_ENV") ?? "qa";

    private static TestSettings Build()
    {
        var env = CurrentEnvironment;
        var baseDir = AppContext.BaseDirectory;

        var builder = new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
            .AddUserSecrets(typeof(ConfigFactory).Assembly, optional: true)
            .AddEnvironmentVariables(prefix: "PTS_");

        var root = builder.Build();
        var settings = new TestSettings();
        root.Bind(settings);

        Validate(settings, env);
        return settings;
    }

    private static void Validate(TestSettings s, string env)
    {
        if (string.IsNullOrWhiteSpace(s.Applications.Member.BaseUrl))
            throw new InvalidOperationException(
                $"Applications:Member:BaseUrl is not configured for environment '{env}'.");

        if (string.IsNullOrWhiteSpace(s.Applications.Admin.BaseUrl))
            throw new InvalidOperationException(
                $"Applications:Admin:BaseUrl is not configured for environment '{env}'.");
    }
}
