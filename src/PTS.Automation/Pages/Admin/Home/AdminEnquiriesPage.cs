using PTS.Automation.Pages.Admin;

namespace PTS.Automation.Pages.Admin.Home;

/// <summary>
/// PTS staff Sales / Enquiries landing (<c>AdminController.Index</c> →
/// <c>/Admin/Index</c>). Uses <c>_AdminLayout.cshtml</c>.
/// </summary>
public sealed class AdminEnquiriesPage : AdminPage
{
    public AdminEnquiriesPage(IPage page, AppUrl app) : base(page, app) { }

    public override string RelativePath => "Admin/Index";

    /// <summary>Desktop admin chrome — present on every admin layout page.</summary>
    private ILocator AdminMenuHeader => Page.Locator("header.top_header.admin-menu").First;

    protected override ILocator ReadinessIndicator => AdminMenuHeader;

    public Task<bool> IsAdminHeaderVisibleAsync() => AdminMenuHeader.IsVisibleAsync();
}
