using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.TrustAccounts;

/// <summary>
/// Bank → Trust Accounts landing. Source: <c>Views/Admin/TrustAccounts.cshtml</c>,
/// menu: <c>AdminMenu.cshtml</c> → <c>AdminController.TrustAccounts</c>.
/// </summary>
public sealed class AdminTrustAccountsPage : AdminPage
{
    public AdminTrustAccountsPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.TrustAccounts;

    /// <summary>Balance link opens <c>Admin/AdminTransactions</c> in a new tab (<c>target="_blank"</c>).</summary>
    private ILocator FirstBalanceLink =>
        Page.Locator("section.bankTrustedAccount a.underline.pts-text-blue").First;

    protected override ILocator ReadinessIndicator =>
        Page.Locator("section.bankTrustedAccount").First;

    /// <summary>Opens the first trust account balance drill-down; returns the new <see cref="IPage"/> tab.</summary>
    public async Task<IPage> OpenFirstAccountTransactionsInNewTabAsync()
    {
        await FirstBalanceLink.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = Settings.Timeouts.DefaultMs
        });
        return await Page.RunAndWaitForPopupAsync(async () => await FirstBalanceLink.ClickAsync());
    }

    public Task<int> CountAccountBlocksAsync() => Page.Locator("section.bankTrustedAccount div.profile_detail").CountAsync();
}
