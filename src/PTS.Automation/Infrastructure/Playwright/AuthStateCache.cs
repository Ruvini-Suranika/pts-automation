namespace PTS.Automation.Infrastructure.Playwright;

/// <summary>
/// Caches Playwright <c>storageState</c> files per role so every test doesn't have
/// to log in through the UI. The first test for a given role performs a UI login
/// and writes the storage state to disk; all subsequent tests for that role
/// inherit the authenticated session.
///
/// The cache is per-run: state files live under <c>TestSettings.Paths.AuthStateDir</c>
/// and are cleared by <c>dotnet clean</c> or by deleting the <c>.auth</c> folder.
/// </summary>
public static class AuthStateCache
{
    private static readonly object _lock = new();
    private static readonly HashSet<string> _primed = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Serialises the prime-auth-state flow across parallel NUnit fixtures.
    /// Without this, two <c>*Test</c> fixtures (e.g. BookingListTests +
    /// TransactionsTests) can reach <see cref="IsPrimed"/> simultaneously,
    /// both return <c>false</c>, both run a UI login, and both race to write
    /// the same <c>storage-state.json</c> file — yielding
    /// <c>IOException: being used by another process</c>. The semaphore
    /// is held for the duration of one role's login; other fixtures wait,
    /// then see <see cref="IsPrimed"/> is true and return immediately.
    /// </summary>
    public static readonly SemaphoreSlim PrimeLock = new(1, 1);

    public static string PathFor(TestSettings s, string role)
    {
        Directory.CreateDirectory(s.Paths.AuthStateDir);
        return Path.Combine(s.Paths.AuthStateDir, $"{role.ToLowerInvariant()}.storage-state.json");
    }

    public static bool IsPrimed(string role)
    {
        lock (_lock) return _primed.Contains(role);
    }

    public static void MarkPrimed(string role)
    {
        lock (_lock) _primed.Add(role);
    }

    public static void Invalidate(string role)
    {
        lock (_lock)
        {
            _primed.Remove(role);
            var path = PathFor(ConfigFactory.Settings, role);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
