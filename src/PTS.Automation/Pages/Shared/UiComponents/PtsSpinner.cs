namespace PTS.Automation.Pages.Shared.UiComponents;

/// <summary>
/// The PTS WebUI shows a <c>.loading</c> overlay during most AJAX calls
/// (initial page load, grid fetches, form submits). Every interaction that
/// triggers server work should wait for this overlay to disappear before
/// asserting, or results will be flaky.
///
/// Central selector so that if the app swaps the overlay's class, only this
/// file changes.
/// </summary>
public sealed class PtsSpinner
{
    private readonly IPage _page;
    private readonly TestSettings _settings;

    public PtsSpinner(IPage page)
    {
        _page = page;
        _settings = ConfigFactory.Settings;
    }

    private ILocator Overlay => _page.Locator(".loading, .loading-overlay, .spinner-overlay");

    /// <summary>
    /// Waits for the overlay to be gone. Returns immediately if no overlay is
    /// present (i.e. nothing to wait on).
    /// </summary>
    public async Task WaitUntilHiddenAsync(int? timeoutMs = null)
    {
        if (await Overlay.CountAsync() == 0) return;

        await Overlay.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs ?? _settings.Timeouts.NavigationMs
        });
    }

    public Task<bool> IsVisibleAsync() =>
        Overlay.CountAsync().ContinueWith(t =>
            t.Result > 0 && Overlay.First.IsVisibleAsync().GetAwaiter().GetResult());
}
