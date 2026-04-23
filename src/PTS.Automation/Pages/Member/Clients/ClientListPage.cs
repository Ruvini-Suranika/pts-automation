using PTS.Automation.Pages.Member.Shell;
using PTS.Automation.Pages.Shared.UiComponents;

namespace PTS.Automation.Pages.Member.Clients;

/// <summary>
/// Member → Clients landing page (the searchable clients grid + Add Client modal).
/// Source view: <c>Views/Client/ClientSearchView.cshtml</c>.
/// Controller action: <c>ClientController.ClientSearchView</c>.
///
/// The page layout has two "modes":
///   - filter panel visible, grid (<c>#ClientMainSection</c>) hidden until a search runs
///   - grid visible after the user clicks Search (or the page-load AJAX populates it)
/// Our readiness probe is the Add Client button, which is always visible.
/// </summary>
public sealed class ClientListPage : MemberPage
{
    public ClientListPage(IPage page, AppUrl app) : base(page, app)
    {
        AddClient = new AddClientModal(page);
        Grid = new PtsTable(page, "#clientsTableBody");
    }

    public override string RelativePath => MemberRoutes.Clients;

    // ── Nested components ───────────────────────────────────────────────────
    public AddClientModal AddClient { get; }
    public PtsTable Grid { get; }

    // ── Locators ────────────────────────────────────────────────────────────
    private ILocator AddClientButton => Page.Locator("button[data-bs-target='#Addclient']");
    private ILocator ImportDataLink  => Page.Locator("a[data-bs-target='#ImportData']");

    private ILocator FirstNameFilter => Page.Locator("#txtFirstName");
    private ILocator LastNameFilter  => Page.Locator("#txtLastName");
    private ILocator EmailFilter     => Page.Locator("#txtEmail");
    private ILocator PhoneFilter     => Page.Locator("#txtTelephone");
    private ILocator EnquiryFilter   => Page.Locator("#dropDownEnquiry");
    private ILocator AssignedFilter  => Page.Locator("#dropDownAssignUser");
    private ILocator SearchButton    => Page.Locator("#filterButton");

    private ILocator GridSection     => Page.Locator("#ClientMainSection");
    private ILocator GridSearch      => Page.Locator("#search");

    protected override ILocator ReadinessIndicator => AddClientButton;

    // ── Queries ─────────────────────────────────────────────────────────────
    public Task<bool> IsAddClientButtonVisibleAsync() => AddClientButton.IsVisibleAsync();
    public Task<bool> IsImportDataLinkVisibleAsync()  => ImportDataLink.IsVisibleAsync();
    public Task<bool> IsSearchButtonVisibleAsync()    => SearchButton.IsVisibleAsync();

    // ── Actions ─────────────────────────────────────────────────────────────
    /// <summary>Opens the Add Client modal and returns it ready for interaction.</summary>
    public async Task<AddClientModal> OpenAddClientAsync()
    {
        await AddClientButton.ClickAsync();
        await AddClient.WaitForOpenAsync();
        return AddClient;
    }

    public async Task FilterByEmailAsync(string email)
    {
        await EmailFilter.FillAsync(email);
        await SearchButton.ClickAsync();
        await Spinner.WaitUntilHiddenAsync();
    }

    /// <summary>
    /// The grid is populated via an AJAX call to
    /// <c>/Client/GetSearchClientDetailinclientsearch</c>. Use this to block
    /// until the grid has responded before asserting on rows.
    /// </summary>
    public async Task WaitForGridLoadedAsync()
    {
        await Page.WaitForResponseAsync(
            r => r.Url.Contains("GetSearchClientDetailinclientsearch", StringComparison.OrdinalIgnoreCase)
                 && r.Status == 200,
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });

        await Spinner.WaitUntilHiddenAsync();
    }
}
