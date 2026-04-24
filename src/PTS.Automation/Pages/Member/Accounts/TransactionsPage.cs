using PTS.Automation.Pages.Member.Shell;
using PTS.Automation.Pages.Shared.UiComponents;

namespace PTS.Automation.Pages.Member.Accounts;

/// <summary>
/// Member → Accounts → Transactions page.
/// Source view: <c>Views/Client/Transactions.cshtml</c>.
/// Controller action: <c>ClientController.Transactions</c>
/// (this one lives under Client, unlike its Accounts siblings which live
/// under TravelMemberAccount — caught us out naming the POM).
/// JS: <c>Scripts/PTSApp/AccountTransactions.js</c>.
///
/// <para>
/// Key differences vs. PaymentsOutstanding / PaymentDue:
/// </para>
/// <list type="bullet">
///   <item>The page does NOT auto-fire <c>loadData()</c> on document ready.
///         The user has to click the Search / Filter button (the dev JS calls
///         <c>validateDateRange()</c> first). So <see cref="WaitForReadyAsync"/>
///         only proves the filter panel is visible.</item>
///   <item>Search endpoint is <b>GET</b> (<c>/Client/TransactionsResults</c>),
///         not POST — differs from A1/A2.</item>
///   <item><c>validateDateRange()</c> rejects the click unless at least one
///         of: Period != 0, Currency selected, or an optional filter
///         (AdditionalNarrative / CustomerReference / PaymentReference) has
///         a value. <see cref="SearchWithBroadPeriodAsync"/> handles that.</item>
/// </list>
///
/// <para>
/// The three-dots menu (<c>#filterCorner</c>) exposes four export / print
/// actions: Copy to clipboard, Print, Download PDF, Download Excel.
/// </para>
/// </summary>
public sealed class TransactionsPage : MemberPage
{
    public const string SearchEndpointFragment = "/Client/TransactionsResults";

    public TransactionsPage(IPage page, AppUrl app) : base(page, app)
    {
        Grid = new PtsTable(page, "#transactionsresult");
    }

    public override string RelativePath => MemberRoutes.Transactions;

    // ── Components ─────────────────────────────────────────────────────────
    public PtsTable Grid { get; }

    // ── Filter panel ────────────────────────────────────────────────────────
    private ILocator FilterPanel             => Page.Locator("#advanceFilter");
    private ILocator PeriodSelect            => Page.Locator("#Period");
    private ILocator FromDateInput           => Page.Locator("#FromDate");
    private ILocator ToDateInput             => Page.Locator("#ToDate");
    private ILocator TrnTypeSelect           => Page.Locator("#TRNtype");
    private ILocator CurrencySelect          => Page.Locator("#Currency");
    private ILocator MinValueInput           => Page.Locator("#MinValue");
    private ILocator MaxValueInput           => Page.Locator("#MaxValue");
    private ILocator AdditionalNarrativeInput => Page.Locator("#AdditionalNarrative");
    private ILocator PayTypeSelect           => Page.Locator("#PayType");
    private ILocator TravelFromDateInput     => Page.Locator("#TravelFromDate");
    private ILocator TravelToDateInput       => Page.Locator("#TravelToDate");
    private ILocator CustomerReferenceInput  => Page.Locator("#CustomerReference");
    private ILocator PaymentReferenceInput   => Page.Locator("#PaymentReference");
    private ILocator SearchButton            => FilterPanel.Locator("pts-button[type='search']").First;
    private ILocator ClearFiltersButton      => FilterPanel.Locator("pts-button[type='clear-filters']").First;

    // ── Grid data section ──────────────────────────────────────────────────
    private ILocator GridSearchInput         => Page.Locator("#search");
    private ILocator GridTable               => Page.Locator("#transactionDetails");
    private ILocator HeaderCells             => GridTable.Locator("thead tr th");

    // ── Three-dots (export / print) menu ───────────────────────────────────
    private ILocator ThreeDotsToggle         => Page.Locator("#filterCorner");
    private ILocator CopyToClipboardOption   => Page.Locator("a.dropdown-item[onclick*='copytoClipboard']");
    private ILocator PrintOption             => Page.Locator("a.dropdown-item[onclick*='printTransactionData']");
    private ILocator DownloadPdfOption       => Page.Locator("#TransactionToPdf");
    private ILocator DownloadExcelOption     => Page.Locator("#TransactionToExcel");

    /// <summary>
    /// Readiness probe: the Filter (Search) button is rendered up-front even
    /// before the Period dropdown finishes loading via AJAX, so it's the
    /// earliest reliable signal that the static chrome is in place.
    /// </summary>
    protected override ILocator ReadinessIndicator => SearchButton;

    // ── Queries ─────────────────────────────────────────────────────────────
    public Task<bool> IsFilterPanelVisibleAsync()  => FilterPanel.IsVisibleAsync();
    public Task<bool> IsSearchButtonVisibleAsync() => SearchButton.IsVisibleAsync();
    public Task<bool> IsGridSearchVisibleAsync()   => GridSearchInput.IsVisibleAsync();
    public Task<bool> IsThreeDotsToggleVisibleAsync() => ThreeDotsToggle.IsVisibleAsync();

    /// <summary>Returns the trimmed text of every rendered column header.</summary>
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

    /// <summary>The anchor in the first row's Booking Ref column (if rendered).</summary>
    public async Task<ILocator> FirstRowBookingLinkAsync()
    {
        var col = await ColumnIndexOfAsync("Booking Ref");
        return Grid.Rows.First.Locator("td").Nth(col - 1).Locator("a").First;
    }

    /// <summary>The anchor in the first row's Customer Ref column (if rendered).</summary>
    public async Task<ILocator> FirstRowCustomerLinkAsync()
    {
        var col = await ColumnIndexOfAsync("Customer Ref");
        return Grid.Rows.First.Locator("td").Nth(col - 1).Locator("a").First;
    }

    // ── Three-dots menu queries ────────────────────────────────────────────
    public Task<bool> IsCopyToClipboardVisibleAsync() => CopyToClipboardOption.IsVisibleAsync();
    public Task<bool> IsPrintVisibleAsync()           => PrintOption.IsVisibleAsync();
    public Task<bool> IsDownloadPdfVisibleAsync()     => DownloadPdfOption.IsVisibleAsync();
    public Task<bool> IsDownloadExcelVisibleAsync()   => DownloadExcelOption.IsVisibleAsync();

    public Task<string?> GetDownloadPdfHrefAsync()   => DownloadPdfOption.GetAttributeAsync("href");
    public Task<string?> GetDownloadExcelHrefAsync() => DownloadExcelOption.GetAttributeAsync("href");

    // ── Actions ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Opens the three-dots dropdown. Bootstrap toggles <c>show</c> on click;
    /// the menu items then become visible for assertion.
    /// </summary>
    public Task OpenThreeDotsMenuAsync() => ThreeDotsToggle.ClickAsync();

    /// <summary>
    /// Types into the inline grid Search box (client-side filter — no AJAX).
    /// <para>
    /// The dev JS (<c>$("#search").keyup(...)</c>) hides non-matching rows on
    /// every keyup. <see cref="ILocator.FillAsync"/> only dispatches
    /// <c>input</c>, so we additionally dispatch <c>keyup</c>.
    /// </para>
    /// </summary>
    public async Task SetGridSearchAsync(string text)
    {
        await GridSearchInput.FillAsync(text);
        await GridSearchInput.DispatchEventAsync("keyup");
    }

    /// <summary>
    /// Picks filter values that will satisfy the dev-side
    /// <c>validateDateRange()</c> check so that clicking Search actually fires
    /// <c>loadData()</c>.
    /// <para>
    /// The validator trips if either:
    /// </para>
    /// <list type="bullet">
    ///   <item>No optional filter AND Period == "0" AND no valid date range
    ///         → <c>"Please enter date range"</c> error.</item>
    ///   <item>No optional filter AND Currency == "0" (loose equality)
    ///         → <c>"Please Select Currency"</c> error.</item>
    /// </list>
    /// <para>
    /// We pick the first option on BOTH Period and Currency whose value is not
    /// "0" / empty. selectpicker wraps both natives with <c>display:none</c>,
    /// so we set the value via the selectpicker API directly (the same path
    /// a real user's click through the widget would take) and then fire
    /// <c>change</c> so the page's listeners react.
    /// </para>
    /// </summary>
    public async Task SelectBroadPeriodAsync()
    {
        // Period loads async from /Common/GetTransactionPeriod — wait for it.
        await Page.WaitForFunctionAsync(
            "() => (document.querySelector('#Period')?.options?.length ?? 0) >= 2",
            null,
            new PageWaitForFunctionOptions { Timeout = Settings.Timeouts.NavigationMs });

        // Set BOTH Period and Currency to a non-"0" option. We use a single
        // EvaluateAsync so the whole flip happens atomically and uses the
        // real selectpicker API — bypasses the hidden-element actionability
        // problems we'd hit via Playwright's SelectOption on a display:none
        // native select.
        const string script = @"
            () => {
                const pick = (id) => {
                    const sel = document.querySelector(id);
                    if (!sel) return null;
                    let chosen = null;
                    for (const opt of sel.options) {
                        const v = opt.value;
                        if (v !== '0' && v !== '' && v != null) { chosen = v; break; }
                    }
                    if (!chosen && sel.options.length > 0) chosen = sel.options[0].value;
                    const $sel = window.$ ? window.$(sel) : null;
                    if ($sel && typeof $sel.selectpicker === 'function') {
                        $sel.selectpicker('val', chosen);
                    } else {
                        sel.value = chosen;
                    }
                    sel.dispatchEvent(new Event('change', { bubbles: true }));
                    return chosen;
                };
                return { period: pick('#Period'), currency: pick('#Currency') };
            }";
        await Page.EvaluateAsync(script);
    }

    /// <summary>
    /// Clicks Search (the no-id pts-button in the filter panel) and waits
    /// for the <c>/Client/TransactionsResults</c> GET to complete. Returns
    /// (HTTP status, request URL).
    /// </summary>
    public async Task<(int Status, string RequestUrl)> SearchAndCaptureAsync()
    {
        var requestTask = Page.WaitForRequestAsync(
            r => r.Url.Contains(SearchEndpointFragment, StringComparison.OrdinalIgnoreCase)
                 && r.Method == "GET",
            new PageWaitForRequestOptions { Timeout = Settings.Timeouts.NavigationMs });

        await SearchButton.ClickAsync();
        var request  = await requestTask;
        var response = await request.ResponseAsync();
        await Spinner.WaitUntilHiddenAsync();
        return (response?.Status ?? 0, request.Url);
    }

    /// <summary>
    /// After <see cref="SearchAndCaptureAsync"/> completes, waits for the
    /// grid to repaint. Uses NetworkIdle + Spinner same as A1/A2 — the
    /// request event has already fired by the time we get here.
    /// </summary>
    public async Task WaitForGridPaintedAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle,
            new PageWaitForLoadStateOptions { Timeout = Settings.Timeouts.NavigationMs });
        await Spinner.WaitUntilHiddenAsync();
    }
}
