using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.TrustAccounts;

/// <summary>
/// Trust account transaction reporting (<c>Admin/AdminTransactions</c>). Opened from Trust Accounts
/// balance link. Source: <c>Views/Admin/AdminTransactions.cshtml</c>; grid HTML from AJAX
/// <c>/Admin/TransactionsResultsAll/</c> (<c>TrustAccounts.js</c>).
/// </summary>
public sealed class AdminTrustAccountTransactionsPage : AdminPage
{
    public AdminTrustAccountTransactionsPage(IPage page, AppUrl app) : base(page, app) { }

    /// <summary>Not used for <see cref="BasePage.GotoAsync"/> — this page is opened via popup from Trust Accounts.</summary>
    public override string RelativePath => AdminRoutes.AdminTransactionsPathPrefix;

    private ILocator PeriodSelect => Page.Locator("select#Period");
    private ILocator TransactionsTbody => Page.Locator("#transactionsresult tbody");
    private ILocator BookingRefLinks => Page.Locator("#transactionsresult a#clientRefrence");

    protected override ILocator ReadinessIndicator => Page.Locator("#hdnTrustAccountId");

    /// <summary>
    /// Hidden field proves the view model is bound; <see cref="BasePage.WaitForReadyAsync"/> uses
    /// <see cref="WaitForSelectorState.Visible"/> which never succeeds for <c>type="hidden"</c> inputs.
    /// </summary>
    public override async Task WaitForReadyAsync()
    {
        await Page.Locator("#hdnTrustAccountId").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Attached,
            Timeout = Settings.Timeouts.DefaultMs
        });
        await Spinner.WaitUntilHiddenAsync();
    }

    /// <summary>
    /// Runs default search: pick first non-placeholder period (disables date requirement in JS) and submit.
    /// </summary>
    public async Task SubmitSearchWithFirstPeriodAsync()
    {
        await PeriodSelect.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Attached,
            Timeout = Settings.Timeouts.DefaultMs
        });

        var optionCount = await PeriodSelect.Locator("option").CountAsync();
        if (optionCount > 1)
            await PeriodSelect.SelectOptionAsync(new SelectOptionValue { Index = 1 });

        // Custom element from AdminTransactions.cshtml — not always exposed as role=button.
        await Page.Locator("pts-button[type=\"search\"]").First.ClickAsync();

        await Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/TransactionsResultsAll", StringComparison.OrdinalIgnoreCase)
                 && r.Request.Method == "POST",
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });
    }

    public ILocator BookingReferenceLinks => BookingRefLinks;

    public Task<int> CountBookingReferenceLinksAsync() => BookingRefLinks.CountAsync();
}
