using PTS.Automation.Pages.Member.Shell;
using PTS.Automation.Pages.Shared.UiComponents;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Member → Accounts → Payment Due page.
/// Source view: <c>Views/TravelMemberAccount/PaymentDue.cshtml</c>.
/// Controller action: <c>TravelMemberAccountController.PaymentDue</c>.
/// JS: <c>Scripts/PTSApp/travelMemberAccount/PaymentDue.js</c>.
///
/// <para>
/// This page is intentionally hidden from the top nav (the menu item carries
/// the <c>d-none</c> class). Reach it either via direct URL or by clicking
/// "Outstanding" → "Due" toggle on the <see cref="PaymentsOutstandingPage"/>.
/// </para>
///
/// On document ready the JS:
///   1. hides <c>#OpenEmailModal</c> (Send Reminder) and <c>#filterCorner</c>
///   2. calls <c>loadData(1)</c> which POSTs to
///      <c>/TravelMemberAccount/GetSearchPaymentDue</c>
///   3. on success, shows <c>#OpenEmailModal</c>, <c>#filterCorner</c> and
///      the tbody, then renders rows
///
/// The Search button has NO id in the view — it's identified by
/// <c>pts-button[type='search']</c> scoped to the filter panel, distinct from
/// the sibling clear-filters pts-button.
/// </summary>
public sealed class PaymentDuePage : MemberPage
{
    public const string SearchEndpointFragment = "/TravelMemberAccount/GetSearchPaymentDue";

    public PaymentDuePage(IPage page, AppUrl app) : base(page, app)
    {
        Grid = new PtsTable(page, "#PaymentDue");
    }

    public override string RelativePath => MemberRoutes.PaymentDue;

    // ── Components ─────────────────────────────────────────────────────────
    public PtsTable Grid { get; }

    // ── Filter panel ────────────────────────────────────────────────────────
    private ILocator FilterPanel        => Page.Locator("#advanceFilter");
    private ILocator FromDateInput      => Page.Locator("#FromDate");
    private ILocator ToDateInput        => Page.Locator("#ToDate");
    private ILocator PaymentTypeSelect  => Page.Locator("#PaymentDuePaymentType");
    private ILocator SearchButton       => FilterPanel.Locator("pts-button[type='search']").First;
    private ILocator ClearFiltersButton => FilterPanel.Locator("pts-button[type='clear-filters']").First;

    // ── Data section controls ───────────────────────────────────────────────
    private ILocator GridSearchInput    => Page.Locator("#searchDue");
    private ILocator SendReminderButton => Page.Locator("#OpenEmailModal");
    private ILocator FilterCornerButton => Page.Locator("#filterCorner");

    // The native input is masked by custom checkbox styling and may be sized
    // to 0×0 / positioned off-viewport. We expose the input itself for
    // checked-state queries, but click the *label* that wraps it (which is
    // what a real user clicks).
    private ILocator CheckAllInput      => Page.Locator("#checkAll");
    private ILocator CheckAllLabel      => Page.Locator("label.checkboxContainer:has(#checkAll)").First;
    private ILocator EmailComposeModal  => Page.Locator("#addemailtemplate");

    // ── Grid table ──────────────────────────────────────────────────────────
    private ILocator GridTable          => Page.Locator("#tblPaymentDue");
    private ILocator HeaderCells        => GridTable.Locator("thead tr th");
    private ILocator AllRowCheckboxes   => Page.Locator("#PaymentDue input[type='checkbox']");

    /// <summary>Row selection checkboxes rendered by PaymentDue.js (<c>class="AllPaymentDue1"</c>).</summary>
    private ILocator PaymentDueRowSelectionCheckboxes => Page.Locator("input.AllPaymentDue1");

    // Outstanding/Due toggle. The "Outstanding" radio is a NAV link to
    // PaymentsOutstanding; "Due" is checked by default on this page.
    private ILocator DueRadio          => Page.Locator("#Percentage");
    private ILocator OutstandingNavLink => Page.Locator("a:has(label.form-check-label[for='Amount'])").First;

    /// <summary>
    /// Readiness probe: the Payment Type select is always rendered in the
    /// (always visible) filter panel. Send Reminder etc. only appear AFTER
    /// the initial loadData() AJAX completes — use <see cref="WaitForGridLoadedAsync"/>
    /// when you need that.
    /// </summary>
    protected override ILocator ReadinessIndicator => PaymentTypeSelect;

    // ── Queries ─────────────────────────────────────────────────────────────
    public Task<bool> IsSendReminderVisibleAsync()    => SendReminderButton.IsVisibleAsync();
    public Task<bool> IsEmailComposeModalVisibleAsync() =>
        Page.Locator("#addemailtemplate.show").IsVisibleAsync();
    public Task<bool> IsCheckAllLabelVisibleAsync()   => CheckAllLabel.IsVisibleAsync();
    public Task<bool> IsCheckAllCheckedAsync()        => CheckAllInput.IsCheckedAsync();
    public Task<int>  RowCheckboxCountAsync()         => AllRowCheckboxes.CountAsync();

    /// <summary>Count of per-row payment-due checkboxes (excludes header <c>#checkAll</c>).</summary>
    public Task<int> AssignableRowCheckboxCountAsync() => PaymentDueRowSelectionCheckboxes.CountAsync();

    /// <summary>Selects the first <c>AllPaymentDue1</c> row checkbox (force-click; masked native input).</summary>
    public Task SelectFirstAssignableRowCheckboxAsync() =>
        PaymentDueRowSelectionCheckboxes.First.ClickAsync(new LocatorClickOptions { Force = true });

    /// <summary>
    /// Count of row checkboxes currently in the <c>:checked</c> state.
    /// Chains <c>:checked</c> into the selector itself rather than as a
    /// nested locator (which would look for <c>:checked</c> *descendants* of
    /// each checkbox and always return 0).
    /// </summary>
    public Task<int> CheckedRowCountAsync() =>
        Page.Locator("#PaymentDue input[type='checkbox']:checked").CountAsync();

    /// <summary>Returns the trimmed text of every column header (in DOM order).</summary>
    public async Task<IReadOnlyList<string>> GetColumnHeadersAsync() =>
        (await HeaderCells.AllInnerTextsAsync())
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

    /// <summary>1-based column index whose header matches (case-insensitive). -1 if absent.</summary>
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

    /// <summary>The first row's BOOKING column anchor (rendered by JS after AJAX).</summary>
    public ILocator FirstRowBookingLink() =>
        Grid.Rows.First.Locator("td a").First;

    /// <summary>The row checkbox in the first data row (skips the header checkbox).</summary>
    public ILocator FirstRowCheckbox() =>
        Grid.Rows.First.Locator("input[type='checkbox']").First;

    // ── Actions ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Selects an option in the Payment Type dropdown. Same selectpicker quirk
    /// as <see cref="PaymentsOutstandingPage"/> — Force=true + dispatch change.
    /// </summary>
    public async Task SelectPaymentTypeAsync(string label)
    {
        await PaymentTypeSelect.SelectOptionAsync(
            new SelectOptionValue { Label = label },
            new LocatorSelectOptionOptions { Force = true });
        await PaymentTypeSelect.DispatchEventAsync("change");
    }

    /// <summary>
    /// Types into the inline grid Search box (client-side filter; dev JS
    /// hooks <c>keyup</c> on <c>#searchDue</c>, so we dispatch it explicitly).
    /// </summary>
    public async Task SetGridSearchAsync(string text)
    {
        await GridSearchInput.FillAsync(text);
        await GridSearchInput.DispatchEventAsync("keyup");
    }

    /// <summary>Clicks the Send Reminder button (no-op safe; behaviour depends on selection state).</summary>
    public Task ClickSendReminderAsync() => SendReminderButton.ClickAsync();

    /// <summary>
    /// Toggles the header "select all" checkbox.
    ///
    /// <para>
    /// We can't use <c>ClickAsync</c> on the native input — it's positioned
    /// off-viewport / sized 0×0 by the custom checkbox styling, and even
    /// <c>Force=true</c> still fails the viewport bounds check.
    /// </para>
    /// <para>
    /// We can't reliably use <c>ClickAsync</c> on the wrapping
    /// <c>&lt;label&gt;</c> either — depending on browser, that may toggle
    /// the visual style without dispatching a real <c>change</c> event on
    /// the input, leaving the dev's <c>$("#checkAll").change(...)</c>
    /// handler unfired and the row checkboxes unticked.
    /// </para>
    /// <para>
    /// The robust path is to call the native input's own <c>click()</c>
    /// method via JS — that toggles <c>checked</c> AND fires the trusted
    /// <c>change</c> event in one shot, exactly like a real user click would.
    /// </para>
    /// </summary>
    public Task ClickCheckAllAsync() =>
        CheckAllInput.EvaluateAsync("el => el.click()");

    /// <summary>
    /// Force-clicks the first row's checkbox. Force is required because the
    /// native checkbox is visually masked by a custom label/span in the
    /// .checkboxContainer styling.
    /// </summary>
    public Task ClickFirstRowCheckboxAsync() =>
        FirstRowCheckbox().ClickAsync(new LocatorClickOptions { Force = true });

    /// <summary>
    /// Clicks Search (via the no-id pts-button) and waits for the
    /// <c>/TravelMemberAccount/GetSearchPaymentDue</c> POST to complete.
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

    /// <summary>
    /// Waits for the initial <c>loadData(1)</c> AJAX to have finished — same
    /// trick as <see cref="PaymentsOutstandingPage.WaitForGridLoadedAsync"/>:
    /// use NetworkIdle + spinner instead of WaitForResponse, since the request
    /// fires before this POM can subscribe.
    /// </summary>
    public async Task WaitForGridLoadedAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle,
            new PageWaitForLoadStateOptions { Timeout = Settings.Timeouts.NavigationMs });
        await Spinner.WaitUntilHiddenAsync();
    }
}
