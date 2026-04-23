namespace PTS.Automation.Infrastructure.Playwright;

/// <summary>
/// Builds <see cref="BrowserNewContextOptions"/> based on <see cref="TestSettings"/>.
/// Used by fixtures to configure per-test browser contexts (viewport, video,
/// storage state for pre-authenticated sessions, etc.).
/// </summary>
public static class ContextOptionsFactory
{
    public static BrowserNewContextOptions Build(TestSettings s, string? storageStatePath = null)
    {
        var opts = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width  = s.Browser.Viewport.Width,
                Height = s.Browser.Viewport.Height
            },
            IgnoreHTTPSErrors = true,
            AcceptDownloads = true,
            RecordVideoDir = ShouldRecordVideo(s) ? s.Paths.VideoDir : null,
            RecordVideoSize = new RecordVideoSize
            {
                Width  = s.Browser.Viewport.Width,
                Height = s.Browser.Viewport.Height
            }
        };

        if (!string.IsNullOrWhiteSpace(storageStatePath) && File.Exists(storageStatePath))
        {
            opts.StorageStatePath = storageStatePath;
        }

        return opts;
    }

    private static bool ShouldRecordVideo(TestSettings s) =>
        string.Equals(s.Browser.Video, "on", StringComparison.OrdinalIgnoreCase)
        || string.Equals(s.Browser.Video, "retain-on-failure", StringComparison.OrdinalIgnoreCase);
}
