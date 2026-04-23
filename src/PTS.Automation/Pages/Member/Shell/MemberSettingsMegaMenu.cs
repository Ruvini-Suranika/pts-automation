namespace PTS.Automation.Pages.Member.Shell;

/// <summary>
/// The "gear icon" settings mega-menu in the top bar. Source:
/// <c>MemberMegaMenu.cshtml</c>. Items here are role/permission gated — the
/// "API supplier settings" entry only appears if the user has certain flags,
/// so its <c>GoTo…</c> method may not be valid for every account.
///
/// Trigger: <c>a#settingDropdown</c> (expands the dropdown panel).
/// </summary>
public sealed class MemberSettingsMegaMenu
{
    private readonly IPage _page;
    private readonly AppUrl _app;

    public MemberSettingsMegaMenu(IPage page, AppUrl app)
    {
        _page = page;
        _app = app;
    }

    // ── Locators ───────────────────────────────────────────────────────
    // Scope to the <li class="MegaMenu"> whose header (desktop / mobile) is
    // currently rendered — same pattern as MemberProfileMenu. The panel
    // itself isn't `:visible` until the toggle is clicked, so we can't filter
    // the panel directly.
    private ILocator Wrapper => _page.Locator("li.MegaMenu:visible").First;
    private ILocator Toggle  => Wrapper.Locator("a#settingDropdown");
    private ILocator Panel   => Wrapper.Locator("div.dropdown-menu[aria-labelledby='settingDropdown']");

    // ── Queries ────────────────────────────────────────────────────────
    public Task<bool> IsToggleVisibleAsync() => Toggle.IsVisibleAsync();

    // ── Actions ────────────────────────────────────────────────────────
    public async Task OpenAsync()
    {
        await Toggle.ClickAsync();
        await Panel.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Returns the list of currently-visible menu item labels. Useful as a
    /// sanity check that the permissions you expect are present for the test
    /// user. Opens the panel if it isn't already.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetVisibleItemLabelsAsync()
    {
        if (!await Panel.IsVisibleAsync()) await OpenAsync();

        return await Panel
            .Locator("li .MegaInnerLink > label")
            .AllInnerTextsAsync();
    }

    public Task GoToApiSupplierSettingsAsync()  => ClickItemAsync("API supplier settings");
    public Task GoToContractedSuppliersAsync()  => ClickItemAsync("Contracted Suppliers");
    public Task GoToDownloadsAsync()            => ClickItemAsync("Downloads");
    public Task GoToEmailSettingsAsync()        => ClickItemAsync("Email Settings");
    public Task GoToItinerarySettingsAsync()    => ClickItemAsync("Itinerary settings");
    public Task GoToOrganisationSettingsAsync() => ClickItemAsync("Organisation");
    public Task GoToQuoteSettingsAsync()        => ClickItemAsync("Quote Settings");
    public Task GoToUsersAsync()                => ClickItemAsync("Users");

    private async Task ClickItemAsync(string label)
    {
        await OpenAsync();
        await Panel.Locator("li").Filter(new() { HasText = label }).First.Locator("a").First.ClickAsync();
    }
}
