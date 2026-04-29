namespace PTS.Automation.Pages.Admin.Settings;

/// <summary>Admin → Settings → Users → Admin users (<c>Admin/AdminUsers</c>). Rows from <c>POST /Admin/GetAllAdminUsers</c>.</summary>
public sealed class AdminAdminUsersListPage : AdminPage
{
    public AdminAdminUsersListPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => AdminRoutes.AdminUsers;

    protected override ILocator ReadinessIndicator => Page.Locator("#linkAdminUserActive");

    public ILocator AdminUserTableBody => Page.Locator("#tableAdminUserBody");

    public ILocator TwoFaColumnHeader =>
        Page.Locator("table.table thead th", new() { HasText = "2FA ACTIVATED" }).First;

    public Task<IResponse> StartWaitForGetAllAdminUsersAsync() =>
        Page.WaitForResponseAsync(
            r => r.Url.Contains("/Admin/GetAllAdminUsers", StringComparison.OrdinalIgnoreCase)
                 && string.Equals(r.Request.Method, "POST", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = Settings.Timeouts.NavigationMs });
}
