namespace PTS.Automation.Pages.Admin.Settings;

/// <summary>Admin → Settings → Users → Supplier users (<c>Admin/SupplierUsers</c>). Rows from <c>POST /Admin/GetAllSupplierUser</c>.</summary>
public sealed class AdminSupplierUsersListPage : AdminPage
{
    public AdminSupplierUsersListPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.SupplierUsers;

    protected override ILocator ReadinessIndicator => Page.Locator("#linkSupplierUserActive");

    /// <summary>Supplier list reuses <c>#tableAdminUserBody</c>; scope to the supplier article.</summary>
    public ILocator SupplierUserTableBody =>
        Page.Locator("article:has(#linkSupplierUserActive) #tableAdminUserBody");

    public ILocator TwoFaColumnHeader =>
        Page.Locator("article:has(#linkSupplierUserActive) thead th", new() { HasText = "2FA ACTIVATED" })
            .First;

    public Task<IResponse> StartWaitForGetAllSupplierUsersAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/GetAllSupplierUser", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });
}
