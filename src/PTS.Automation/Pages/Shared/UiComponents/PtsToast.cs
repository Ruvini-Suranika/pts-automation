namespace PTS.Automation.Pages.Shared.UiComponents;

/// <summary>
/// Flash / toast messages shown after form submits. The PTS WebUI uses a mix
/// of Bootstrap toasts and the dev team's own <c>pts-alert</c> component, so
/// this helper unions the common selectors rather than pinning to one.
///
/// Usage:
/// <code>
///     var toast = new PtsToast(Page);
///     var msg   = await toast.WaitForAsync(PtsToastKind.Success);
///     Assert.That(msg, Does.Contain("Client added"));
/// </code>
/// </summary>
public sealed class PtsToast
{
    private readonly IPage _page;
    private readonly TestSettings _settings;

    public PtsToast(IPage page)
    {
        _page = page;
        _settings = ConfigFactory.Settings;
    }

    // Common union of success / error toast containers seen in the app.
    private ILocator Success => _page.Locator(
        ".toast-success, .alert-success, .pts-alert--success, #successToast.show").First;

    private ILocator Error => _page.Locator(
        ".toast-error, .alert-danger, .pts-alert--error, #errorToast.show").First;

    private ILocator Any => _page.Locator(
        ".toast.show, .alert.show, .pts-alert--success, .pts-alert--error").First;

    /// <summary>
    /// Waits for a toast of the given kind to appear and returns its trimmed text.
    /// Throws if no matching toast appears within the default timeout.
    /// </summary>
    public async Task<string> WaitForAsync(PtsToastKind kind, int? timeoutMs = null)
    {
        var locator = kind switch
        {
            PtsToastKind.Success => Success,
            PtsToastKind.Error   => Error,
            _ => Any
        };

        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? _settings.Timeouts.DefaultMs
        });

        return (await locator.InnerTextAsync()).Trim();
    }

    /// <summary>Returns the text of any currently-visible toast, or <c>null</c>.</summary>
    public async Task<string?> GetCurrentTextAsync() =>
        await Any.CountAsync() > 0 && await Any.IsVisibleAsync()
            ? (await Any.InnerTextAsync()).Trim()
            : null;
}

public enum PtsToastKind
{
    Any,
    Success,
    Error
}
