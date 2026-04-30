using System.IO;
using System.Runtime.CompilerServices;

namespace PTS.Automation.Properties;

/// <summary>
/// Runs as soon as this assembly loads, before NUnit/Allure hooks. Ensures Allure reads
/// <c>allureConfig.json</c> from the test output folder and writes <c>allure-results</c> there
/// (VSTest often sets <see cref="Environment.CurrentDirectory"/> to the project or solution root).
/// </summary>
internal static class AllurePathBootstrap
{
    [ModuleInitializer]
    internal static void AlignAllurePaths()
    {
        var baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var configPath = Path.Combine(baseDir, "allureConfig.json");
        if (File.Exists(configPath))
            Environment.SetEnvironmentVariable("ALLURE_CONFIG", configPath);

        Environment.CurrentDirectory = baseDir;
    }
}
