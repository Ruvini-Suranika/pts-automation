namespace PTS.Automation.Pages.Admin.Applying;

/// <summary>Admin Sales tab — <c>Admin/EditMemberCompany/{id}</c> (<c>EditMemberCompany.cshtml</c>).</summary>
public sealed class AdminEditMemberCompanySalesPage : AdminPage
{
    public AdminEditMemberCompanySalesPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath =>
        throw new InvalidOperationException("Use GotoWithMemberIdAsync.");

    protected override ILocator ReadinessIndicator =>
        Page.Locator(".profile_header span.h1", new() { HasText = "Details" }).First;

    public ILocator AddTrustAccountTrigger => Page.Locator("#add_trust_account_ID");

    public ILocator AddTrustAccountModal => Page.Locator("#add_trust_account");

    public ILocator AddTrustAccountDropdown => AddTrustAccountModal.Locator("#addAccountDropDown");

    public ILocator TrustAccountDetailsTable => Page.Locator("th", new() { HasText = "Trust Account Details" }).First;

    /// <summary>Assign / edit virtual account actions in the trust grid (icons with tooltip titles from markup).</summary>
    public ILocator TrustRowActionLinks =>
        Page.Locator("a[onclick*=\"openVirtualAccountModal\"]");

    public async Task GotoWithMemberIdAsync(long memberId)
    {
        var path = $"{AdminRoutes.EditMemberCompanyPathPrefix}/{memberId}";
        await Page.GotoAsync(new Uri(App.BaseUri, path).ToString(),
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForReadyAsync();
    }

    public Task OpenAddTrustAccountModalAsync() => AddTrustAccountTrigger.ClickAsync();
}
