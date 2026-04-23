using PTS.Automation.Pages.Member.Shell;
using PTS.Automation.Pages.Shared.UiComponents;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Member → Accounts → Payments (Outstanding) page.
/// Source view: <c>Views/TravelMemberAccount/PaymentsOutstanding.cshtml</c>.
/// Controller action: <c>TravelMemberAccountController.PaymentsOutstanding</c>.
/// JS: <c>Scripts/PTSApp/travelMemberAccount/PaymentOutstanding.js</c>.
///
/// On document ready, the page calls <c>loadData()</c> which POSTs to
/// <c>/TravelMemberAccount/GetSearchPaymentOutstanding</c> and renders rows
/// into <c>#PaymentOutstanding</c>.
///
/// Sub-nav (Overview / Transactions / GPS / <b>Payments</b> / Unclaimed /
/// Unassigned / Profit) sits in <c>section.table_header.topMenuList</c>.
/// </summary>
public sealed class PaymentsOutstandingPage : MemberPage
{
    // NOTE: The dev code has TWO candidate endpoints:
    //   - /TravelMemberAccount/GetSearchPaymentOutstanding  (bindDataOutStandingSearch — dead code path)
    //   - /TravelMemberAccount/GetAllPaymentByNumberOfPages (loadData — the one actually called by
    //     the Search button's onclick and by the initial page-load)
    // We watch the latter because it's what the app actually hits.
    public const string SearchEndpointFragment = "/TravelMemberAccount/GetAllPaymentByNumberOfPages";

    public PaymentsOutstandingPage(IPage page, AppUrl app) : base(page, app)
    {
        Grid = new PtsTable(page, "#PaymentOutstanding");
    }

    public override string RelativePath => MemberRoutes.PaymentsOutstanding;

    // ── Components ─────────────────────────────────────────────────────────
    public PtsTable Grid { get; }

    // ── Top sub-nav (links from this page) ─────────────────────────────────
    private ILocator OverviewTab     => Page.Locator("section.topMenuList a.overview");
    private ILocator TransactionsTab => Page.Locator("section.topMenuList a.transaction");
    private ILocator PaymentsTab     => Page.Locator("#OutstandingPaymentLink");
    private ILocator UnclaimedTab    => Page.Locator("section.topMenuList a.unclaimed");
    private ILocator UnassignedTab   => Page.Locator("section.topMenuList a.unassigned");
    private ILocator ProfitTab       => Page.Locator("section.topMenuList a.profit");

    // ── Outstanding / Due toggle (Due is a NAV link to PaymentDue) ─────────
    private ILocator OutstandingRadio => Page.Locator("#Amount");
    private ILocator DueLink          => Page.Locator("a:has(label.form-check-label[for='Percentage'])").First;

    // ── Filter controls ────────────────────────────────────────────────────
    private ILocator SearchInput        => Page.Locator("#search");
    private ILocator PaymentTypeSelect  => Page.Locator("#OutstandingPaymentType");
    private ILocator SearchButton       => Page.Locator("#btnSearchOutstanding");
    private ILocator SendReminderButton => Page.Locator("#OpenEmailModal");
    private ILocator CheckAllHeader     => Page.Locator("#checkAll");

    // ── Grid ────────────────────────────────────────────────────────────────
    private ILocator GridTable    => Page.Locator("#tblPaymentOutstanding");
    private ILocator HeaderCells  => GridTable.Locator("thead tr th");

    // ── Modals (post-action) ───────────────────────────────────────────────
    private ILocator EmailComposeModal => Page.Locator("#addemailtemplate");

    /// <summary>
    /// Readiness probe: the Search button is always rendered; we additionally
    /// expect <c>loadData()</c> to have fired (or be in flight) — callers can
    /// follow up with <see cref="WaitForGridLoadedAsync"/>.
    /// </summary>
    protected override ILocator ReadinessIndicator => SearchButton;

    // ── Queries ─────────────────────────────────────────────────────────────
    public Task<bool> IsSearchButtonVisibleAsync()    => SearchButton.IsVisibleAsync();
    public Task<bool> IsSendReminderVisibleAsync()    => SendReminderButton.IsVisibleAsync();
    public Task<bool> IsPaymentTypeFilterVisibleAsync() => PaymentTypeSelect.IsVisibleAsync();
    public Task<bool> IsEmailComposeModalVisibleAsync() => EmailComposeModal.IsVisibleAsync();

    /// <summary>Returns the trimmed text of every column header in the grid (in DOM order).</summary>
    public async Task<IReadOnlyList<string>> GetColumnHeadersAsync() =>
        (await HeaderCells.AllInnerTextsAsync())
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

    /// <summary>1-based column index whose header text matches (case-insensitive). -1 if absent.</summary>
    public async Task<int> ColumnIndexOfAsync(string headerText)
    {
        var headers = await HeaderCells.AllInnerTextsAsync();
        for (int i = 0; i < headers.Count; i++)
        {
            if (string.Equals(headers[i].Trim(), headerText, StringComparison.OrdinalIgnoreCase))
                return i + 1;
        }
        return -1;
    }

    /// <summary>The first row's BOOKING column anchor (booking-detail link rendered by the JS).</summary>
    public ILocator FirstRowBookingLink() =>
        Grid.Rows.First.Locator("td a").First;

    /// <summary>The row checkbox in the first row of the grid.</summary>
    public ILocator FirstRowCheckbox() =>
        Grid.Rows.First.Locator("input[type='checkbox']").First;

    // ── Actions ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Selects an option in the Payment Type dropdown by visible label.
    ///
    /// <para>
    /// The native <c>&lt;select id="OutstandingPaymentType"&gt;</c> is wrapped
    /// by Bootstrap-select (<c>selectpicker()</c>) which sets the native
    /// element to <c>display:none</c>. Playwright's default
    /// <c>SelectOptionAsync</c> auto-waits for actionability (including
    /// visibility) — against a hidden native select it hangs until timeout.
    /// </para>
    /// <para>
    /// We therefore pass <c>Force = true</c> to skip the actionability check,
    /// then explicitly dispatch <c>change</c> so selectpicker's own listeners
    /// update the visible widget.
    /// </para>
    /// </summary>
    public async Task SelectPaymentTypeAsync(string label)
    {
        await PaymentTypeSelect.SelectOptionAsync(
            new SelectOptionValue { Label = label },
            new LocatorSelectOptionOptions { Force = true });
        await PaymentTypeSelect.DispatchEventAsync("change");
    }

    /// <summary>Clears the Payment Type dropdown (selects empty/none) — used by validator tests.</summary>
    public async Task ClearPaymentTypeAsync()
    {
        await PaymentTypeSelect.SelectOptionAsync(
            new SelectOptionValue { Value = "" },
            new LocatorSelectOptionOptions { Force = true });
        await PaymentTypeSelect.DispatchEventAsync("change");
    }

    /// <summary>
    /// Types into the inline grid Search box (client-side filter — no AJAX).
    /// The dev JS hooks the <c>keyup</c> event on <c>#search</c>, so
    /// <c>FillAsync</c> alone isn't enough (that only dispatches
    /// <c>input</c>). We fill AND dispatch <c>keyup</c> so the filter handler
    /// actually runs.
    /// </summary>
    public async Task SetGridSearchAsync(string text)
    {
        await SearchInput.FillAsync(text);
        await SearchInput.DispatchEventAsync("keyup");
    }

    /// <summary>
    /// Clicks the Search (pts-button) and waits for the
    /// <c>/TravelMemberAccount/GetSearchPaymentOutstanding</c> POST to complete.
    /// Returns (status, raw POST body).
    /// </summary>
    public async Task<(int Status, string PostData)> SearchAndCaptureAsync()
    {
        var requestTask = Page.WaitForRequestAsync(
            r => r.Url.Contains(SearchEndpointFragment, StringComparison.OrdinalIgnoreCase)
                 && r.Method == "POST",
            new PageWaitForRequestOptions { Timeout = Settings.Timeouts.NavigationMs });

        await SearchButton.ClickAsync();
        var request  = await requestTask;
        var response = await request.ResponseAsync();
        await Spinner.WaitUntilHiddenAsync();
        return (response?.Status ?? 0, request.PostData ?? "");
    }

    /// <summary>Click Send Reminder button. Behaviour depends on selection state.</summary>
    public Task ClickSendReminderAsync() => SendReminderButton.ClickAsync();

    /// <summary>Tick the first row's checkbox.</summary>
    public Task SelectFirstRowAsync() => FirstRowCheckbox().ClickAsync();

    /// <summary>
    /// Waits for the initial <c>loadData()</c> AJAX to have finished. The
    /// page's on-ready JS fires that request BEFORE this POM can subscribe to
    /// the response event, so we cannot wait on the event directly — it would
    /// miss it and hang until timeout. Instead we wait for the network to go
    /// idle and then for the loading overlay to clear. That's sufficient in
    /// practice because the spinner is shown during the AJAX call.
    /// </summary>
    public async Task WaitForGridLoadedAsync()
    {
        // Network-idle is a Playwright built-in: no in-flight requests for 500 ms.
        // If the AJAX has already completed we return instantly; if it's in
        // flight we block until it finishes.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle,
            new PageWaitForLoadStateOptions { Timeout = Settings.Timeouts.NavigationMs });
        await Spinner.WaitUntilHiddenAsync();
    }
}
