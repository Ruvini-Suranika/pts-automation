namespace PTS.Automation.Pages.Shared.UiComponents;

/// <summary>
/// Generic wrapper for any Bootstrap modal in the PTS WebUI.
/// Concrete modal page objects either:
///   - compose a <see cref="PtsModal"/> (when they add a lot of extra fields), or
///   - inherit from it (when they're thin wrappers).
///
/// The root locator is whatever CSS selector identifies the modal container
/// (commonly <c>#SomeModalId</c> or <c>.modal.show</c>).
/// </summary>
public class PtsModal
{
    protected readonly IPage Page;
    protected readonly ILocator Root;
    protected readonly TestSettings Settings;

    public PtsModal(IPage page, string rootSelector)
        : this(page, page.Locator(rootSelector)) { }

    public PtsModal(IPage page, ILocator root)
    {
        Page = page;
        Root = root;
        Settings = ConfigFactory.Settings;
    }

    // ── Locators ───────────────────────────────────────────────────────────
    public ILocator Container       => Root;
    public ILocator Title           => Root.Locator(".modal-title, .modal-header h1, .modal-header h3").First;
    public ILocator CloseIconButton => Root.Locator("button.btn-close, button[data-bs-dismiss='modal']").First;

    // ── Waits ──────────────────────────────────────────────────────────────
    public Task WaitForOpenAsync(int? timeoutMs = null) =>
        Root.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs ?? Settings.Timeouts.DefaultMs
        });

    public Task WaitForClosedAsync(int? timeoutMs = null) =>
        Root.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs ?? Settings.Timeouts.DefaultMs
        });

    // ── Queries ────────────────────────────────────────────────────────────
    public Task<bool> IsOpenAsync() => Root.IsVisibleAsync();

    public async Task<string> GetTitleTextAsync() =>
        (await Title.InnerTextAsync()).Trim();

    // ── Actions ────────────────────────────────────────────────────────────
    public async Task CloseViaIconAsync()
    {
        await CloseIconButton.ClickAsync();
        await WaitForClosedAsync();
    }

    /// <summary>Click a button inside the modal by its visible text.</summary>
    public Task ClickButtonAsync(string buttonText) =>
        Root.GetByRole(AriaRole.Button, new() { Name = buttonText }).ClickAsync();
}
