using PTS.Automation.Pages.Shared.UiComponents;

namespace PTS.Automation.Pages;

/// <summary>
/// Root Page Object. Every concrete page derives from this.
/// Provides lifecycle (<see cref="GotoAsync"/>, <see cref="WaitForReadyAsync"/>)
/// and shared access to cross-cutting UI helpers (toasts, modals, spinner).
///
/// Rule: selectors live ONLY in page classes, never in tests.
/// </summary>
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly AppUrl App;
    protected readonly TestSettings Settings;

    /// <summary>Loading-overlay waiter shared across every page.</summary>
    protected PtsSpinner Spinner { get; }

    /// <summary>Toast / flash-message reader shared across every page.</summary>
    protected PtsToast Toast { get; }

    protected BasePage(IPage page, AppUrl app)
    {
        Page = page;
        App = app;
        Settings = ConfigFactory.Settings;
        Spinner = new PtsSpinner(page);
        Toast = new PtsToast(page);
    }

    /// <summary>Path (relative to <see cref="AppUrl.BaseUrl"/>) that this page lives at.</summary>
    public abstract string RelativePath { get; }

    /// <summary>Readiness probe — concrete pages return a locator that is visible when the page is interactive.</summary>
    protected abstract ILocator ReadinessIndicator { get; }

    public virtual async Task GotoAsync()
    {
        var url = new Uri(App.BaseUri, RelativePath.TrimStart('/')).ToString();
        await Page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    public virtual async Task WaitForReadyAsync()
    {
        await ReadinessIndicator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = Settings.Timeouts.DefaultMs
        });

        await Spinner.WaitUntilHiddenAsync();
    }

    public Task<bool> IsAtAsync() => IsAtUrlAsync(RelativePath);

    protected async Task<bool> IsAtUrlAsync(string relativePath)
    {
        var expected = new Uri(App.BaseUri, relativePath.TrimStart('/')).AbsolutePath.TrimEnd('/');
        var current  = new Uri(Page.Url).AbsolutePath.TrimEnd('/');
        return current.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }
}
